import React, { useRef, useState } from "react";
import { Button, Card } from "@heroui/react";

const MinecraftSkinMapper: React.FC = () => {
  const [uploadedImageSrc, setUploadedImageSrc] = useState<string | null>(null);
  const [finalTexture, setFinalTexture] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const triggerFileUpload = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (event) => {
      if (event.target?.result)
        setUploadedImageSrc(event.target.result as string);
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
        rotate: number;
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
        {
          name: "headFront",
          sx: 8,
          sy: 8,
          sw: 8,
          sh: 8,
          destRow: 1,
          destCol: 1,
          transform: { rotate: -90, flipX: false, flipY: false },
        },
        {
          name: "headLeft",
          sx: 0,
          sy: 8,
          sw: 8,
          sh: 8,
          destRow: 3,
          destCol: 3,
          transform: { rotate: -90, flipX: false, flipY: true },
        },
        {
          name: "headRight",
          sx: 16,
          sy: 8,
          sw: 8,
          sh: 8,
          destRow: 2,
          destCol: 3,
          transform: { rotate: -90, flipX: false, flipY: true },
        },
        {
          name: "headBack",
          sx: 24,
          sy: 8,
          sw: 8,
          sh: 8,
          destRow: 2,
          destCol: 2,
          transform: { rotate: 90, flipX: true, flipY: false },
        },
        {
          name: "headTop",
          sx: 8,
          sy: 0,
          sw: 8,
          sh: 8,
          destRow: 4,
          destCol: 1,
          transform: { rotate: 90, flipX: false, flipY: false },
        },
        {
          name: "headBottom",
          sx: 16,
          sy: 0,
          sw: 8,
          sh: 8,
          destRow: 4,
          destCol: 4,
          transform: { rotate: 90, flipX: false, flipY: false },
        },
        {
          name: "bodyFront",
          sx: 20,
          sy: 20,
          sw: 8,
          sh: 12,
          destRow: 3,
          destCol: 2,
          transform: { rotate: 90, flipX: false, flipY: true },
        },
        {
          name: "bodyLeft",
          sx: 16,
          sy: 20,
          sw: 4,
          sh: 12,
          destRow: 4,
          destCol: 2,
          transform: { rotate: 90, flipX: false, flipY: false },
        },
        {
          name: "bodyRight",
          sx: 28,
          sy: 20,
          sw: 4,
          sh: 12,
          destRow: 1,
          destCol: 2,
          transform: { rotate: 90, flipX: false, flipY: true },
        },
        {
          name: "bodyBack",
          sx: 32,
          sy: 20,
          sw: 8,
          sh: 12,
          destRow: 3,
          destCol: 1,
          transform: { rotate: 90, flipX: false, flipY: false },
        },
        {
          name: "bodyTop",
          sx: 20,
          sy: 16,
          sw: 8,
          sh: 4,
          destRow: 2,
          destCol: 1,
          transform: { rotate: 90, flipX: false, flipY: false },
        },
        {
          name: "bodyBottom",
          sx: 28,
          sy: 16,
          sw: 8,
          sh: 4,
          destRow: 4,
          destCol: 3,
          transform: { rotate: 90, flipX: false, flipY: false },
        },
      ];

      parts.forEach((part) => {
        const destX = (part.destCol - 1) * cellSize;
        const destY = (part.destRow - 1) * cellSize;
        ctx.save();
        ctx.translate(destX + cellSize / 2, destY + cellSize / 2);
        if (part.transform.flipX || part.transform.flipY) {
          ctx.scale(
            part.transform.flipX ? -1 : 1,
            part.transform.flipY ? -1 : 1
          );
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
    <div className="min-h-screen bg-gradient-to-r from-indigo-600 to-purple-600 flex flex-col items-center justify-center p-6">
      <div className="max-w-3xl w-full text-center mb-12">
        <h1 className="text-6xl font-extrabold text-white mb-4 tracking-wider">
          Minecraft Skin Mapper
        </h1>
        <p className="text-xl text-white">
          Transform your Minecraft skin into a custom 1024x1024 avatar texture.
        </p>
      </div>
      <div className="bg-white rounded-lg shadow-xl p-8 w-full max-w-md">
        <Button onClick={triggerFileUpload} className="w-full mb-2">
          Upload Minecraft Skin
        </Button>
        <input
          ref={fileInputRef}
          type="file"
          accept="image/png, image/jpeg"
          onChange={handleFileChange}
          className="hidden"
        />
        {uploadedImageSrc && (
          <div className="mb-2">
            <img
              src={uploadedImageSrc}
              alt="Uploaded Skin"
              className="w-full rounded-md shadow-sm"
            />
          </div>
        )}
        {uploadedImageSrc && (
          <Button onClick={generateTexture} className="w-full mb-5">
            Generate Custom Texture
          </Button>
        )}
        {finalTexture && (
          <Card className="mt-4">
            <img
              src={finalTexture}
              alt="Final Texture"
              className="w-full rounded mb-2"
            />

            <Button as="a" href={finalTexture} download="custom_texture.png">
              Download Texture
            </Button>
          </Card>
        )}
      </div>
    </div>
  );
};

export default MinecraftSkinMapper;
