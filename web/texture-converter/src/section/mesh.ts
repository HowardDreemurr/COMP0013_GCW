// @ts-nocheck
import * as THREE from "three";

/**
 * Creates a custom geometry for the "head" with correct UV coords
 * so each face points at the right sub-region of a 64×64 texture.
 */
export function buildHeadGeometry(): THREE.BufferGeometry {
  // A simple box: 8×8×8 in MC => we scale to e.g. 1×1×1 in the world
  // We'll do 1 in each dimension and apply 1/2 scale if needed
  const width = 1,
    height = 1,
    depth = 1;

  // 8 corners => 24 unique vertices for 6 faces => we must define them w/ correct UV
  // We'll define 6 faces, each face has 2 triangles => 6 * 2 * 3 = 36 indices

  const positions: number[] = [];
  const uvs: number[] = [];
  const indices: number[] = [];

  let vertexCount = 0;

  /**
   * Utility: addFace with 4 corners => 2 triangles => fill positions/uv/indices
   *   corners is an array of [x, y, z, u, v]
   *   faceRect is the sub-region for that face in UV space
   */
  function addFace(
    // corners of the face in 3D
    c1: [number, number, number],
    c2: [number, number, number],
    c3: [number, number, number],
    c4: [number, number, number],
    // each corner's UV in [0..1], e.g. top-left => (u1, v1), top-right => (u2, v1) ...
    uv: [[number, number], [number, number], [number, number], [number, number]]
  ) {
    // p1 => c1 + uv[0], p2 => c2 + uv[1], p3 => c3 + uv[2], p4 => c4 + uv[3]
    // we'll define 2 triangles: (p1, p2, p3), (p1, p3, p4)

    const startIndex = vertexCount;

    // corner 1
    positions.push(c1[0], c1[1], c1[2]);
    uvs.push(uv[0][0], uv[0][1]);

    // corner 2
    positions.push(c2[0], c2[1], c2[2]);
    uvs.push(uv[1][0], uv[1][1]);

    // corner 3
    positions.push(c3[0], c3[1], c3[2]);
    uvs.push(uv[2][0], uv[2][1]);

    // corner 4
    positions.push(c4[0], c4[1], c4[2]);
    uvs.push(uv[3][0], uv[3][1]);

    indices.push(
      startIndex,
      startIndex + 1,
      startIndex + 2,
      startIndex,
      startIndex + 2,
      startIndex + 3
    );
    vertexCount += 4;
  }

  // Pre-calculate half extents
  const hx = width / 2,
    hy = height / 2,
    hz = depth / 2;

  // For the HEAD, let's define the sub-regions in [0..1]
  // e.g. front face: from (8,8)->(16,16) => (0.125,0.125)->(0.25,0.25)
  const uv_front = {
    tl: [0.125, 0.125], // top-left
    tr: [0.25, 0.125], // top-right
    bl: [0.125, 0.25],
    br: [0.25, 0.25],
  };
  // Similarly define back, left, right, top, bottom, etc.
  const uv_back = {
    tl: [0.375, 0.125],
    tr: [0.5, 0.125],
    bl: [0.375, 0.25],
    br: [0.5, 0.25],
  };
  const uv_left = {
    tl: [0.0, 0.125],
    tr: [0.125, 0.125],
    bl: [0.0, 0.25],
    br: [0.125, 0.25],
  };
  const uv_right = {
    tl: [0.25, 0.125],
    tr: [0.375, 0.125],
    bl: [0.25, 0.25],
    br: [0.375, 0.25],
  };
  const uv_top = {
    tl: [0.125, 0.0],
    tr: [0.25, 0.0],
    bl: [0.125, 0.125],
    br: [0.25, 0.125],
  };
  const uv_bottom = {
    tl: [0.25, 0.0],
    tr: [0.375, 0.0],
    bl: [0.25, 0.125],
    br: [0.375, 0.125],
  };

  // FRONT: face z = +hz
  addFace(
    [-hx, hy, hz],
    [hx, hy, hz],
    [hx, -hy, hz],
    [-hx, -hy, hz],
    [uv_front.tl, uv_front.tr, uv_front.br, uv_front.bl]
  );
  // BACK: face z = -hz
  addFace(
    [hx, hy, -hz],
    [-hx, hy, -hz],
    [-hx, -hy, -hz],
    [hx, -hy, -hz],
    [uv_back.tl, uv_back.tr, uv_back.br, uv_back.bl]
  );
  // LEFT: face x = -hx
  addFace(
    [-hx, hy, -hz],
    [-hx, hy, hz],
    [-hx, -hy, hz],
    [-hx, -hy, -hz],
    [uv_left.tl, uv_left.tr, uv_left.br, uv_left.bl]
  );
  // RIGHT: face x = +hx
  addFace(
    [hx, hy, hz],
    [hx, hy, -hz],
    [hx, -hy, -hz],
    [hx, -hy, hz],
    [uv_right.tl, uv_right.tr, uv_right.br, uv_right.bl]
  );
  // TOP: face y = +hy
  addFace(
    [-hx, hy, -hz],
    [hx, hy, -hz],
    [hx, hy, hz],
    [-hx, hy, hz],
    [uv_top.tl, uv_top.tr, uv_top.br, uv_top.bl]
  );
  // BOTTOM: face y = -hy
  addFace(
    [-hx, -hy, hz],
    [hx, -hy, hz],
    [hx, -hy, -hz],
    [-hx, -hy, -hz],
    [uv_bottom.tl, uv_bottom.tr, uv_bottom.br, uv_bottom.bl]
  );

  // Now build a BufferGeometry
  const geom = new THREE.BufferGeometry();
  geom.setAttribute("position", new THREE.Float32BufferAttribute(positions, 3));
  geom.setAttribute("uv", new THREE.Float32BufferAttribute(uvs, 2));
  geom.setIndex(indices);
  geom.computeVertexNormals(); // if you want lighting
  return geom;
}
