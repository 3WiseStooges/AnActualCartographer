"""
Thunderstore icon for AnActualCartographer.

Drawn procedurally rather than resized from source art, so there is no binary master to keep
track of and the icon can be regenerated from this repo alone. Thunderstore wants exactly
256x256 PNG; this renders at 4x and downsamples so the diagonals come out clean.

The motif is the mod: a compass rose with the explored ring pushed out well past the faint
one vanilla gives you.

    python tools/make-icon.py
"""
import math
import os

from PIL import Image, ImageDraw

SS = 4                      # supersampling factor
SIZE = 256
S = SIZE * SS
C = S / 2.0

BG_IN = (32, 49, 47)
BG_OUT = (11, 18, 18)
GOLD = (219, 184, 108)
GOLD_DIM = (138, 111, 58)
PARCHMENT = (238, 222, 186)

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(os.path.dirname(HERE), "icon.png")


def background(img):
    """Radial falloff, dark at the edges like unexplored map fog."""
    draw = ImageDraw.Draw(img)
    steps = int(C)
    for i in range(steps, 0, -1):
        t = i / steps
        colour = tuple(int(BG_OUT[c] * t + BG_IN[c] * (1 - t)) for c in range(3))
        draw.ellipse([C - i, C - i, C + i, C + i], fill=colour)


def ring(overlay, radius, width, colour, alpha):
    draw = ImageDraw.Draw(overlay)
    draw.ellipse(
        [C - radius, C - radius, C + radius, C + radius],
        outline=colour + (alpha,),
        width=width,
    )


def compass(overlay):
    """Eight-point rose, cardinals long, each spike split light/dark down its spine."""
    draw = ImageDraw.Draw(overlay)
    base = 0.042 * S

    for index in range(8):
        angle = math.radians(index * 45)
        length = 0.155 * S if index % 2 == 0 else 0.082 * S

        tip = (C + length * math.sin(angle), C - length * math.cos(angle))
        left = (C + base * math.sin(angle - math.pi / 2), C - base * math.cos(angle - math.pi / 2))
        right = (C + base * math.sin(angle + math.pi / 2), C - base * math.cos(angle + math.pi / 2))

        draw.polygon([(C, C), tip, left], fill=PARCHMENT + (255,))
        draw.polygon([(C, C), tip, right], fill=GOLD_DIM + (255,))

    hub = 0.018 * S
    draw.ellipse([C - hub, C - hub, C + hub, C + hub], fill=GOLD + (255,))


def main():
    img = Image.new("RGB", (S, S), BG_OUT)
    background(img)

    overlay = Image.new("RGBA", (S, S), (0, 0, 0, 0))

    # The range vanilla gives you: close in and easy to miss.
    ring(overlay, 0.205 * S, 3 * SS, GOLD, 105)
    # Two steps out to the range the table buys you, brightest at the edge.
    ring(overlay, 0.305 * S, 4 * SS, GOLD, 165)
    ring(overlay, 0.415 * S, 6 * SS, GOLD, 245)

    compass(overlay)

    # Frame, so the icon still reads as a tile against a pale page.
    ring(overlay, 0.482 * S, 3 * SS, GOLD_DIM, 150)

    img = Image.alpha_composite(img.convert("RGBA"), overlay)
    img.convert("RGB").resize((SIZE, SIZE), Image.LANCZOS).save(OUT)
    print(f"wrote {OUT} ({SIZE}x{SIZE})")


if __name__ == "__main__":
    main()
