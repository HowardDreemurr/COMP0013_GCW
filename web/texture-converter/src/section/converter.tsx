import React, { useEffect, useRef, useState } from "react";
import { Button, Card, Switch } from "@heroui/react";

import maskImg from "../assets/images/texture_mask.png";

const MinecraftSkinMapper: React.FC = () => {
  const [uploadedImageSrc, setUploadedImageSrc] = useState<string | null>(null);
  const [finalTexture, setFinalTexture] = useState<string | null>(null);
  const [applyMask, setApplyMask] = useState<boolean>(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

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

      if (applyMask) {
        const finalImage = new Image();
        finalImage.crossOrigin = "Anonymous";
        finalImage.src = finalCanvas.toDataURL("image/png");
        finalImage.onload = () => {
          const maskImageElement = new Image();
          maskImageElement.crossOrigin = "Anonymous";
          maskImageElement.src = maskImg;
          maskImageElement.onload = () => {
            const maskedCanvas = document.createElement("canvas");
            maskedCanvas.width = 1024;
            maskedCanvas.height = 1024;
            const mCtx = maskedCanvas.getContext("2d");
            if (!mCtx) return;
            mCtx.drawImage(finalImage, 0, 0, 1024, 1024);
            mCtx.globalCompositeOperation = "destination-in";
            mCtx.drawImage(maskImageElement, 0, 0, 1024, 1024);
            setFinalTexture(maskedCanvas.toDataURL("image/png"));
          };
        };
      }

      setTimeout(() => {
        containerRef.current?.scrollTo({
          top: containerRef.current.scrollHeight,
          behavior: "smooth",
        });
      }, 0);
    };
  };

  useEffect(() => {
    generateTexture();
  }, [applyMask]);

  return (
    <div
      ref={containerRef}
      className="min-h-screen bg-gradient-to-r from-indigo-600 to-purple-600 flex flex-col items-center overflow-auto p-6 absolute w-full h-full"
    >
      <div className="max-w-3xl w-full text-center mb-5">
        <h1 className="text-6xl font-extrabold text-white mb-2 tracking-wider">
          Minecraft Skin Mapper
        </h1>
        <p className="text-xl text-white">
          Transform your Minecraft skin into a custom 1024x1024 avatar texture.
        </p>
      </div>
      <div className="bg-white rounded-lg shadow-xl px-8 py-5 w-full max-w-md">
        <Button onClick={triggerFileUpload} className="w-full">
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
          <div className="my-2">
            <img
              src={uploadedImageSrc}
              alt="Uploaded Skin"
              className="w-full rounded-md shadow-sm"
              style={{ imageRendering: "pixelated" }}
            />
          </div>
        )}
        {uploadedImageSrc && (
          <Button onClick={generateTexture} className="w-full mb-2">
            Generate Custom Texture
          </Button>
        )}
        {finalTexture && (
          <div className="bg-white rounded-lg shadow-xl px-8 py-5 w-full max-w-md flex flex-col">
            <img
              src={finalTexture}
              alt="Final Texture"
              className="w-full rounded mb-2"
            />
            {finalTexture && (
              <Switch
                checked={applyMask}
                onChange={() => setApplyMask(!applyMask)}
                className="mb-2"
              >
                Ghosted
              </Switch>
            )}
            <Button as="a" href={finalTexture} download="custom_texture.png">
              Download Texture
            </Button>
          </div>
        )}
      </div>
    </div>
  );
};

export default MinecraftSkinMapper;
