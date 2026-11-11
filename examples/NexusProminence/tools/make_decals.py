"""Make the Nexus Prominence sign and splat textures (PNG) for the generator.

    python make_decals.py <SceneTools>/Assets/Templates/NexusProminence/Tex

Kept out of the repository (size budget): run this before NexusProminenceBuilder.Build.
Signs are original jokes in the Madness Combat spirit; "DON'T BLINK" and the WANTED captions
come from the film's own walls. Needs Pillow and the Windows fonts Impact, Arial Black, Segoe Print.
"""
import math
import os
import random
import sys

from PIL import Image, ImageDraw, ImageFilter, ImageFont

FONTS = "C:/Windows/Fonts/"
RED = (220, 18, 18, 255)
DARK = (28, 26, 28, 255)
PAPER = (222, 220, 214, 255)

# name: (lines, width px, height px, background, ink, font, stripe)
SIGNS = {
    "sign_population": (["WELCOME TO NEVADA", "POPULATION: DECREASING"], 512, 200, DARK, RED, "impact.ttf", True),
    "sign_noloiter": (["NO LOITERING", "NO GRUNTING"], 384, 192, PAPER, DARK, "ariblk.ttf", True),
    "sign_incident": (["DAYS WITHOUT", "AN INCIDENT:", "0"], 384, 320, PAPER, RED, "ariblk.ttf", False),
    "sign_wanted": (["WANTED"], 256, 96, DARK, RED, "impact.ttf", False),
    "sign_reward": (["REWARD: YES"], 256, 80, PAPER, DARK, "impact.ttf", False),
    "sign_number": (["NOW SERVING", "#  9,999"], 384, 220, DARK, RED, "impact.ttf", False),
    "sign_receptionist": (["PLEASE DO NOT SHOOT", "THE RECEPTIONIST", "(AGAIN)"], 512, 240, PAPER, DARK, "ariblk.ttf", True),
    "sign_upside": (["THIS SIDE UP"], 384, 120, PAPER, RED, "impact.ttf", False),
    "sign_fragile": (["FRAGILE", "(LIKE GRUNTS)"], 384, 180, PAPER, RED, "impact.ttf", False),
    "sign_nopeek": (["SNIPER NEST", "NO PEEKING"], 384, 200, DARK, RED, "impact.ttf", True),
    "sign_doorgrunts": (["CAUTION", "SHUTTER CLOSES", "ON GRUNTS"], 384, 260, PAPER, DARK, "ariblk.ttf", True),
    "sign_dontblink": (["DON'T BLINK"], 512, 160, None, (210, 210, 210, 235), "segoeprb.ttf", False),
    "sign_lostfound": (["LOST & FOUND", "1 LEFT ARM", "2 HEADS (UNCLAIMED)"], 384, 260, PAPER, DARK, "ariblk.ttf", False),
    "sign_lunch": (["WHO ATE MY", "LUNCH?!", "- AGENT 47B"], 320, 260, (236, 226, 120, 255), DARK, "segoeprb.ttf", False),
    "sign_coffee": (["COFFEE  1 CR", "DECAF  NO"], 320, 180, DARK, RED, "impact.ttf", False),
    "sign_authorized": (["AUTHORIZED PERSONNEL ONLY", "(YOU ARE NOT)"], 512, 180, DARK, RED, "impact.ttf", True),
    "sign_selfdestruct": (["SELF DESTRUCT", "DO NOT PRESS"], 320, 200, PAPER, RED, "impact.ttf", True),
    "sign_wetfloor": (["CAUTION", "WET FLOOR", "(IT'S RED)"], 256, 320, (240, 200, 40, 255), DARK, "ariblk.ttf", False),
}


def fit_font(draw, lines, font, w, h):
    size = h
    while size > 8:
        f = ImageFont.truetype(FONTS + font, size)
        widths = [draw.textbbox((0, 0), ln, font=f)[2] for ln in lines]
        if max(widths) <= w * 0.86 and size * 1.15 * len(lines) <= h * 0.8:
            return f
        size -= 2
    return ImageFont.truetype(FONTS + font, 8)


def sign(lines, w, h, bg, ink, font, stripe):
    im = Image.new("RGBA", (w, h), bg if bg else (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    if bg:
        d.rectangle([4, 4, w - 5, h - 5], outline=ink, width=6)
    if stripe:
        for x in range(-h, w, 36):  # hazard stripe along the bottom
            d.polygon([(x, h - 4), (x + 18, h - 4), (x + 30, h - 22), (x + 12, h - 22)], fill=RED if ink != RED else DARK)
    f = fit_font(d, lines, font, w, h - (26 if stripe else 0))
    lh = f.size * 1.12
    y = ((h - (24 if stripe else 0)) - lh * len(lines)) / 2
    for ln in lines:
        tw = d.textbbox((0, 0), ln, font=f)[2]
        d.text(((w - tw) / 2, y), ln, font=f, fill=ink)
        y += lh
    return im


def splat(seed, size=256):
    """Blood splat: a blob, droplets and a few streaks, alpha cut."""
    r = random.Random(seed)
    im = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    c = size / 2
    col = (150 + r.randint(0, 40), 0, 4, 255)
    pts = []
    for i in range(24):
        a = i / 24 * math.tau
        rad = size * (0.18 + r.random() * 0.12)
        pts.append((c + math.cos(a) * rad, c + math.sin(a) * rad))
    d.polygon(pts, fill=col)
    for _ in range(18):
        a = r.random() * math.tau
        dist = size * (0.25 + r.random() * 0.22)
        rad = size * (0.01 + r.random() * 0.035)
        x, y = c + math.cos(a) * dist, c + math.sin(a) * dist
        d.ellipse([x - rad, y - rad, x + rad, y + rad], fill=col)
        if r.random() < 0.4:
            d.line([(c + math.cos(a) * size * 0.2, c + math.sin(a) * size * 0.2), (x, y)], fill=col, width=max(2, int(rad)))
    return im.filter(ImageFilter.GaussianBlur(0.8))


def holes(seed, size=256):
    """A burst of bullet holes on transparent ground."""
    r = random.Random(seed)
    im = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for _ in range(9):
        x, y = r.uniform(30, size - 30), r.uniform(30, size - 30)
        for k in range(6):
            a = r.random() * math.tau
            d.line([(x, y), (x + math.cos(a) * 16, y + math.sin(a) * 16)], fill=(20, 20, 20, 200), width=2)
        d.ellipse([x - 7, y - 7, x + 7, y + 7], fill=(8, 8, 8, 255))
    return im


def scrawl(seed, size=(384, 192)):
    """Red spray-paint scribble like the tags on the film's walls (no real letters)."""
    r = random.Random(seed)
    im = Image.new("RGBA", size, (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    w, h = size
    for stroke in range(5):
        x, y = r.uniform(30, 90) + stroke * w * 0.16, r.uniform(h * 0.3, h * 0.7)
        pts = [(x, y)]
        for _ in range(6):
            x += r.uniform(-25, 40)
            y = min(h - 15, max(15, y + r.uniform(-60, 60)))
            pts.append((x, y))
        d.line(pts, fill=RED, width=9, joint="curve")
        # drips
        px, py = pts[-1]
        d.line([(px, py), (px, py + r.uniform(15, 40))], fill=RED, width=4)
    return im


def main(out):
    os.makedirs(out, exist_ok=True)
    for name, spec in SIGNS.items():
        sign(*spec).save(os.path.join(out, name + ".png"))
    for i in range(4):
        splat(10 + i).save(os.path.join(out, "splat_%d.png" % i))
    for i in range(2):
        holes(30 + i).save(os.path.join(out, "holes_%d.png" % i))
    for i in range(2):
        scrawl(50 + i).save(os.path.join(out, "scrawl_%d.png" % i))
    print("wrote", len(os.listdir(out)), "files to", out)


if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else "Tex")
