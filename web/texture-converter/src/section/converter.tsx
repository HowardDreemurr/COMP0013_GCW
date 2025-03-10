import React, { useState } from "react";
import { Button, Card } from "@heroui/react";

const MinecraftSkinMapper: React.FC = () => {
  const [uploadedImageSrc, setUploadedImageSrc] = useState<string | null>(null);
  const [finalTexture, setFinalTexture] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (event) => {
      if (event.target?.result) setUploadedImageSrc(event.target.result as string);
    };
    reader.readAsDataURL(file);
  };

  const generateTexture = () => {
    if (!uploadedImageSrc) return;
    const img = new Image();
    img.src = uploadedImageSrc;
    img.crossOrigin = "Anonymous";
    img.onload = () => {
      const finalCanvas = document.createElement("canvas");
      finalCanvas.width = 1024;
      finalCanvas.height = 1024;
      const ctx = finalCanvas.getContext("2d");
      if (!ctx) return;
      ctx.imageSmoothingEnabled = false;
      const cellSize = 256;

      interface Transform {
        rotate: number; // degrees
        flipX: boolean;
        flipY: boolean;
      }
      interface Part {
        name: string;
        sx: number;
        sy: number;
        sw: number;
        sh: number;
        destRow: number;
        destCol: number;
        transform: Transform;
      }

      const parts: Part[] = [
        { name: "headFront", sx: 8,  sy: 8,  sw: 8,  sh: 8,  destRow: 1, destCol: 1, transform: { rotate: -90, flipX: false, flipY: false } },
        { name: "headLeft",  sx: 0,  sy: 8,  sw: 8,  sh: 8,  destRow: 3, destCol: 3, transform: { rotate: -90, flipX: false, flipY: true } },
        { name: "headRight", sx: 16, sy: 8,  sw: 8,  sh: 8,  destRow: 2, destCol: 3, transform: { rotate: -90, flipX: false, flipY: true } },
        { name: "headBack",  sx: 24, sy: 8,  sw: 8,  sh: 8,  destRow: 2, destCol: 2, transform: { rotate: 90, flipX: true, flipY: false } },
        { name: "headTop",   sx: 8,  sy: 0,  sw: 8,  sh: 8,  destRow: 4, destCol: 1, transform: { rotate: 90, flipX: false, flipY: false } },
        { name: "headBottom",sx: 16, sy: 0,  sw: 8,  sh: 8,  destRow: 4, destCol: 4, transform: { rotate: 90, flipX: false, flipY: false } },
        { name: "bodyFront", sx: 20, sy: 20, sw: 8,  sh: 12, destRow: 3, destCol: 2, transform: { rotate: 90, flipX: false, flipY: true } },
        { name: "bodyLeft",  sx: 16, sy: 20, sw: 4,  sh: 12, destRow: 4, destCol: 2, transform: { rotate: 90, flipX: false, flipY: false } },
        { name: "bodyRight", sx: 28, sy: 20, sw: 4,  sh: 12, destRow: 1, destCol: 2, transform: { rotate: 90, flipX: false, flipY: true } },
        { name: "bodyBack",  sx: 32, sy: 20, sw: 8,  sh: 12, destRow: 3, destCol: 1, transform: { rotate: 90, flipX: false, flipY: false } },
        { name: "bodyTop",   sx: 20, sy: 16, sw: 8,  sh: 4,  destRow: 2, destCol: 1, transform: { rotate: 90, flipX: false, flipY: false } },
        { name: "bodyBottom",sx: 28, sy: 16, sw: 8,  sh: 4,  destRow: 4, destCol: 3, transform: { rotate: 90, flipX: false, flipY: false } }
      ];

      parts.forEach(part => {
        const destX = (part.destCol - 1) * cellSize;
        const destY = (part.destRow - 1) * cellSize;
        ctx.save();
        ctx.translate(destX + cellSize / 2, destY + cellSize / 2);
        if (part.transform.flipX || part.transform.flipY) {
          ctx.scale(part.transform.flipX ? -1 : 1, part.transform.flipY ? -1 : 1);
        }
        ctx.rotate((part.transform.rotate * Math.PI) / 180);
        ctx.drawImage(
          img,
          part.sx,
          part.sy,
          part.sw,
          part.sh,
          -cellSize / 2,
          -cellSize / 2,
          cellSize,
          cellSize
        );
        ctx.restore();
      });

      setFinalTexture(finalCanvas.toDataURL("image/png"));
    };
  };

  return (
    <div style={{ padding: "2rem", textAlign: "center" }}>
      <h1>Minecraft Skin → Custom 1024x1024 Mapper</h1>
      <input
        type="file"
        accept="image/png, image/jpeg"
        onChange={handleFileChange}
        style={{ marginBottom: "1rem" }}
      />
      <div>
        <Button onClick={generateTexture} disabled={!uploadedImageSrc} style={{ marginBottom: "1rem" }}>
          Generate Custom Texture
        </Button>
      </div>
      {finalTexture && (
        <Card style={{ display: "inline-block", textAlign: "center" }}>
          <img src={finalTexture} alt="Final Texture" style={{ width: "100%" }} />
          <div style={{ marginTop: "1rem" }}>
            <Button as="a" href={finalTexture} download="custom_texture.png">
              Download Texture
            </Button>
          </div>
        </Card>
      )}
    </div>
  );
};

export default MinecraftSkinMapper;