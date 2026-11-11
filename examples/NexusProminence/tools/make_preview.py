# Workshop preview: the ten room shots (NexusShots.Render -shotDir shots) as a building cross-section.
# Run from the folder that holds shots/: python make_preview.py -> preview.jpg
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import glob, os
import re; shots = sorted(p for p in glob.glob("shots/Room_*.png") if not re.search(r"_(BL|BR|FL|FR).png$", p))
names = ["STREET","WANTED HALL","CONTROLS","CARGO BAY","SNIPER LOFT","GARAGE","SPOTLIGHTS","LOCKERS","BREAK ROOM","CONTROL TOWER"]
W=H=1024; bg=(14,12,13); red=(225,20,20)
im=Image.new("RGB",(W,H),bg); d=ImageDraw.Draw(im)
def font(sz):
    for f in ["C:/Windows/Fonts/impact.ttf","C:/Windows/Fonts/arialbd.ttf"]:
        if os.path.exists(f): return ImageFont.truetype(f,sz)
    return ImageFont.load_default()
tw,th=316,178; gap=14
rows=[[0,1,2],[3,4,5],[6,7,8],[9]]  # street bottom, tower top
top=150
# building outline
d.rectangle([12,top-14,W-12,H-12],fill=(30,28,30),outline=(60,58,60),width=4)
for ri,row in enumerate(rows):
    y=H-24-(ri+1)*(th+gap+18)+gap
    xs=[(W-(len(row)*tw+(len(row)-1)*gap))//2+i*(tw+gap) for i in range(len(row))]
    if ri%2==1: row=row[::-1]
    for x,k in zip(xs,row):
        s=Image.open(shots[k]).convert("RGB")
        sw,sh=s.size; c=s.crop((int(sw*0.04),int(sh*0.08),int(sw*0.96),int(sh*0.78))).resize((tw,th),Image.LANCZOS)
        im.paste(c,(x,y))
        d.rectangle([x-3,y-3,x+tw+2,y+th+2],outline=(8,8,8),width=5)
        d.rectangle([x,y+th-4,x+tw,y+th],fill=red)
        lab=f"{k+1}  {names[k]}"; f=font(20)
        d.rectangle([x+6,y+6,x+12+d.textlength(lab,font=f),y+32],fill=(10,10,10))
        d.text((x+9,y+7),lab,font=f,fill=(240,240,240))
    # floor slab with hazard stripes
    sy=y+th+6
    for i in range(24,W-24,28): d.rectangle([i,sy,i+14,sy+8],fill=red if (i//28)%2==0 else (20,20,20))
f=font(92); t="NEXUS PROMINENCE"
d.text(((W-d.textlength(t,font=f))//2+4,14),t,font=f,fill=(60,0,0))
d.text(((W-d.textlength(t,font=f))//2,10),t,font=f,fill=red)
f2=font(28); t2="10 ROOMS  -  STREET TO CONTROL TOWER  -  PLAYGROUND MAP"
d.text(((W-d.textlength(t2,font=f2))//2,108),t2,font=f2,fill=(230,230,230))
im.save("preview.jpg",quality=88); print(os.path.getsize("preview.jpg"))
