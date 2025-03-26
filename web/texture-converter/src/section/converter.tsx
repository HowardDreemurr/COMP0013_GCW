import React, { useEffect, useMemo, useRef, useState } from "react";
import { Button, Switch } from "@heroui/react";
import maskImg from "../assets/images/texture_mask.png";

// 3D / React Three Fiber
import { Canvas, useFrame, useLoader } from "@react-three/fiber";
// @ts-ignore - missing types
import { OrbitControls } from "@react-three/drei";
import * as THREE from "three";
import { FBXLoader } from "three/examples/jsm/loaders/FBXLoader";

/** Python API address */
const API_URL = "http://127.0.0.1:5001/";

/** Utility to read a file as base64 */
function readFileAsBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

/** Minimal model: only a HEAD box & BODY box */
function HeadAndBody({ textureURL }: { textureURL: string }) {
  const groupRef = useRef<THREE.Group>(null);
  const texture = useLoader(THREE.TextureLoader, textureURL);
  // Load FBX model (adjust the path to your FBX file)
  const torseModel = useLoader(FBXLoader, "/models/torse.fbx");
  const headModel = useLoader(FBXLoader, "/models/head.fbx");

  React.useEffect(() => {
    if (torseModel) {
      torseModel.traverse((child: any) => {
        if (child.isMesh) {
          child.material.map = texture;
          child.material.needsUpdate = true;
        }
      });
    }
    if (headModel) {
      headModel.traverse((child: any) => {
        if (child.isMesh) {
          child.material.map = texture;
          child.material.needsUpdate = true;
        }
      });
    }
  }, [torseModel, texture, headModel]);

  if (!texture || !headModel || !torseModel) return null;

  return (
    <group ref={groupRef} position={[0, -0.2, 0]}>
      <primitive object={headModel} texture={texture}></primitive>
      <primitive object={torseModel} texture={texture}></primitive>
    </group>
  );
}

const MinecraftSkinMapper: React.FC = () => {
  const [uploadedImageSrc, setUploadedImageSrc] = useState<string | null>(null);
  const [finalTexture, setFinalTexture] = useState<string | null>(null);
  const [applyMask, setApplyMask] = useState<boolean>(false);

  // For merging multiple skins
  const [showMergeModal, setShowMergeModal] = useState(false);
  const [mergeFiles, setMergeFiles] = useState<FileList | null>(null);
  const [mergePreviews, setMergePreviews] = useState<string[]>([]);

  const fileInputRef = useRef<HTMLInputElement>(null);

  /**
   * Trigger a single-skin file upload
   */
  const triggerFileUpload = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = async (event) => {
      if (event.target?.result) {
        const imgSrc = event.target.result as string;
        setUploadedImageSrc(imgSrc);
        handleGenerateLocalTexture(imgSrc);
      }
    };
    reader.readAsDataURL(file);
  };

  /**
   * Convert 64×64 => custom 1024×1024
   * using partial logic from your older snippet
   */
  const generateCustomTexture = (skinDataURL: string) => {
    const img = new Image();
    img.src = skinDataURL;
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
          transform: { rotate: -90, flipX: false, flipY: true },
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
          transform: { rotate: 0, flipX: false, flipY: true },
        },
        {
          name: "headBottom",
          sx: 16,
          sy: 0,
          sw: 8,
          sh: 8,
          destRow: 4,
          destCol: 4,
          transform: { rotate: 180, flipX: false, flipY: false },
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
          transform: { rotate: 90, flipX: false, flipY: true },
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
          transform: { rotate: 90, flipX: false, flipY: true },
        },
        {
          name: "bodyTop",
          sx: 20,
          sy: 16,
          sw: 8,
          sh: 4,
          destRow: 2,
          destCol: 1,
          transform: { rotate: 0, flipX: false, flipY: true },
        },
        {
          name: "bodyBottom",
          sx: 28,
          sy: 16,
          sw: 8,
          sh: 4,
          destRow: 4,
          destCol: 3,
          transform: { rotate: 180, flipX: false, flipY: false },
        },
        {
          name: "hand_0",
          sx: 48,
          sy: 16,
          sw: 4,
          sh: 4,
          destRow: 1,
          destCol: 3,
          transform: { rotate: 0, flipX: false, flipY: true },
        },
        {
          name: "hand_1",
          sx: 48,
          sy: 16,
          sw: 4,
          sh: 4,
          destRow: 2,
          destCol: 4,
          transform: { rotate: 0, flipX: false, flipY: true },
        },
      ];

      // Draw each part
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

      let finalURL = finalCanvas.toDataURL("image/png");
      if (applyMask) {
        const finalImage = new Image();
        finalImage.src = finalURL;
        finalImage.onload = () => {
          const maskCanvas = document.createElement("canvas");
          maskCanvas.width = 1024;
          maskCanvas.height = 1024;
          const mCtx = maskCanvas.getContext("2d");
          if (!mCtx) return;
          mCtx.drawImage(finalImage, 0, 0, 1024, 1024);
          mCtx.globalCompositeOperation = "destination-in";

          const maskImgTag = new Image();
          maskImgTag.src = maskImg;
          maskImgTag.onload = () => {
            mCtx.drawImage(maskImgTag, 0, 0, 1024, 1024);
            finalURL = maskCanvas.toDataURL("image/png");
            setFinalTexture(finalURL);
          };
        };
      } else {
        setFinalTexture(finalURL);
      }
    };
  };

  /**
   * Local button to convert user-uploaded 64×64 to 1024×1024
   */
  const handleGenerateLocalTexture = (imgSrc?: string) => {
    const source = imgSrc || uploadedImageSrc;
    if (!source) return;
    generateCustomTexture(source);
  };

  // Re-convert if user toggles "Ghosted"
  useEffect(() => {
    if (uploadedImageSrc) {
      generateCustomTexture(uploadedImageSrc);
    }
  }, [applyMask]);

  // 1) Random from server => returns 64×64 => partial logic
  const handleGenerateRandomTextureVAE = async () => {
    try {
      const res = await fetch(`${API_URL}api/vae/random`, { method: "POST" });
      const data = await res.json();
      const skin64 = `data:image/png;base64,${data.base64}`;
      setUploadedImageSrc(skin64);
      generateCustomTexture(skin64);
    } catch (err) {
      console.error(err);
      alert("Error generating random texture from server.");
    }
  };

  const handleGenerateVariationVAE = async () => {
    if (!uploadedImageSrc) {
      alert("No base skin to vary. Please upload first.");
      return;
    }
    // Check if the uploaded image has the data URL prefix
    const base64Image = uploadedImageSrc.startsWith("data:")
      ? uploadedImageSrc.split(",")[1]
      : uploadedImageSrc;
    try {
      const res = await fetch(`${API_URL}api/vae/variation`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ image: base64Image, noise: 0.5 }),
      });
      const data = await res.json();
      const newSkin64 = `data:image/png;base64,${data.base64}`;
      setUploadedImageSrc(newSkin64);
      generateCustomTexture(newSkin64);
    } catch (err) {
      console.error(err);
      alert("Error generating variation from server.");
    }
  };

  // 3) Merge => returns 64×64 => partial logic
  const handleMergeSkinsVAE = async () => {
    if (!mergeFiles || mergeFiles.length < 2) {
      alert("Need at least 2 skins to merge.");
      return;
    }
    try {
      const base64Images: string[] = [];
      for (let i = 0; i < mergeFiles.length; i++) {
        const file = mergeFiles[i];
        const b64 = await readFileAsBase64(file);
        base64Images.push(b64.split(",")[1]);
      }
      const res = await fetch(`${API_URL}api/vae/merge`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ images: base64Images, noise: 0.2 }),
      });
      const data = await res.json();
      const merged64 = `data:image/png;base64,${data.base64}`;
      setUploadedImageSrc(merged64);
      generateCustomTexture(merged64);
      setShowMergeModal(false);
    } catch (err) {
      console.error(err);
      alert("Error merging skins from server.");
    }
  };

  const handleMergeFilesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setMergeFiles(e.target.files);
      const previews = Array.from(e.target.files).map((file) =>
        URL.createObjectURL(file)
      );
      setMergePreviews(previews);
    }
  };

  return (
    <div className="w-full h-full flex flex-col">
      {/* Title always at top */}
      <div className="bg-gray-800 text-white p-4 text-3xl font-bold">
        Minecraft Skin Mapper
      </div>

      {/* 2-column layout; left is not scrollable, right is flexible */}
      <div className="flex flex-row flex-1 h-full">
        {/* Left Column: compact area, no scroll */}
        <div className="w-80 bg-white p-4 flex flex-col space-y-3">
          {/* Single-skin upload */}
          <Button onClick={triggerFileUpload} className="w-full">
            Upload 64×64 Skin
          </Button>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/png, image/jpeg"
            onChange={handleFileChange}
            className="hidden"
          />

          {uploadedImageSrc && (
            <img
              src={uploadedImageSrc}
              alt="Uploaded 64×64"
              className="rounded border"
              style={{ imageRendering: "pixelated" }}
            />
          )}

          {uploadedImageSrc && (
            <>
              <Button
                onClick={() => handleGenerateLocalTexture()}
                className="w-full"
              >
                Generate Custom Texture
              </Button>
              <Button onClick={handleGenerateVariationVAE} className="w-full">
                Variation
              </Button>
            </>
          )}
          <Button onClick={handleGenerateRandomTextureVAE} className="w-full">
            Random
          </Button>
          <Button onClick={() => setShowMergeModal(true)} className="w-full">
            Merge
          </Button>

          <div className="flex flex-row items-center space-x-2">
            <Switch
              checked={applyMask}
              onChange={() => setApplyMask(!applyMask)}
            >
              Ghosted
            </Switch>
          </div>

          {finalTexture && (
            <>
              <a
                href={finalTexture}
                download="custom_texture.png"
                className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded text-center"
              >
                Download Texture
              </a>
              <img
                src={finalTexture}
                alt="Final 1024×1024"
                className="rounded border absolute bottom-4 right-4 w-64 h-64 bg-gray-600 opacity-90"
              />
            </>
          )}
        </div>

        {/* Right Column: big area with 3D preview of HEAD & BODY using finalTexture */}
        <div className="flex-1 bg-gray-100 p-4">
          <h2 className="text-xl font-bold mb-2">3D Preview (Head & Body)</h2>
          <div style={{ width: "100%", height: "600px" }}>
            <Canvas camera={{ position: [0, 1.2, 2.5], fov: 50 }}>
              <ambientLight />
              <directionalLight position={[2, 5, 3]} />
              <OrbitControls enableZoom={true} enablePan={true} autoRotate />
              {finalTexture && <HeadAndBody textureURL={finalTexture} />}
            </Canvas>
          </div>
        </div>
      </div>

      {/* Merge Modal */}
      {showMergeModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg max-w-md w-full space-y-4">
            <h2 className="text-2xl font-bold">Merge Skins</h2>
            <p>Upload 2+ skins to merge.</p>
            <input
              type="file"
              accept="image/png, image/jpeg"
              multiple
              onChange={handleMergeFilesChange}
              className="w-full"
            />
            {mergePreviews.length > 0 && (
              <div className="flex space-x-2 mt-2">
                {mergePreviews.map((src, idx) => (
                  <img
                    key={idx}
                    src={src}
                    alt={`Preview ${idx}`}
                    className="w-16 h-16 object-cover rounded border"
                  />
                ))}
              </div>
            )}
            <div className="flex space-x-4">
              <Button onClick={handleMergeSkinsVAE} className="flex-1">
                Merge
              </Button>
              <Button
                onClick={() => setShowMergeModal(false)}
                className="flex-1"
              >
                Cancel
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MinecraftSkinMapper;
