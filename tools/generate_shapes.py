"""Generate GLB files for each shape type used by ShapeSpawner.

Each shape is a unit-sized mesh (fits in a 1×1×1 bounding box) with
POSITION, NORMAL, TEXCOORD_0 attributes and a PBR material named 'mat'.
The app scales them to ShapeSize at runtime.

Shapes: cube.glb (copy of template), sphere.glb, cylinder.glb, cone.glb, pyramid.glb
"""

import json
import math
import struct
import os


def write_glb(out_path, positions, normals, texcoords, indices):
    """Write a GLB file from mesh data arrays."""
    num_verts = len(positions) // 3
    num_indices = len(indices)

    # Pack binary
    idx_bytes = struct.pack(f'<{num_indices}I', *indices)
    pos_bytes = struct.pack(f'<{len(positions)}f', *positions)
    norm_bytes = struct.pack(f'<{len(normals)}f', *normals)
    uv_bytes = struct.pack(f'<{len(texcoords)}f', *texcoords)

    def pad4(data):
        remainder = len(data) % 4
        return data + b'\x00' * (4 - remainder) if remainder else data

    idx_bytes = pad4(idx_bytes)
    pos_bytes = pad4(pos_bytes)
    norm_bytes = pad4(norm_bytes)
    uv_bytes = pad4(uv_bytes)

    buffer_data = idx_bytes + pos_bytes + norm_bytes + uv_bytes

    idx_offset = 0
    pos_offset = len(idx_bytes)
    norm_offset = pos_offset + len(pos_bytes)
    uv_offset = norm_offset + len(norm_bytes)

    # Compute bounds
    pos_min = [min(positions[i::3]) for i in range(3)]
    pos_max = [max(positions[i::3]) for i in range(3)]

    gltf = {
        "asset": {"version": "2.0", "generator": "generate_shapes.py"},
        "scene": 0,
        "scenes": [{"nodes": [0]}],
        "nodes": [{"name": "root", "mesh": 0}],
        "meshes": [{"name": "shape_mesh", "primitives": [{
            "attributes": {"POSITION": 1, "NORMAL": 2, "TEXCOORD_0": 3},
            "indices": 0, "material": 0, "mode": 4
        }]}],
        "materials": [{"name": "mat", "pbrMetallicRoughness": {
            "baseColorFactor": [1.0, 1.0, 1.0, 1.0],
            "metallicFactor": 0.0, "roughnessFactor": 1.0
        }, "doubleSided": False}],
        "accessors": [
            {"bufferView": 0, "componentType": 5125, "count": num_indices,
             "type": "SCALAR", "max": [num_verts - 1], "min": [0]},
            {"bufferView": 1, "componentType": 5126, "count": num_verts,
             "type": "VEC3", "max": pos_max, "min": pos_min},
            {"bufferView": 2, "componentType": 5126, "count": num_verts,
             "type": "VEC3", "max": [1.0, 1.0, 1.0], "min": [-1.0, -1.0, -1.0]},
            {"bufferView": 3, "componentType": 5126, "count": num_verts,
             "type": "VEC2", "max": [1.0, 1.0], "min": [0.0, 0.0]},
        ],
        "bufferViews": [
            {"buffer": 0, "byteOffset": idx_offset, "byteLength": len(idx_bytes), "target": 34963},
            {"buffer": 0, "byteOffset": pos_offset, "byteLength": len(pos_bytes), "target": 34962},
            {"buffer": 0, "byteOffset": norm_offset, "byteLength": len(norm_bytes), "target": 34962},
            {"buffer": 0, "byteOffset": uv_offset, "byteLength": len(uv_bytes), "target": 34962},
        ],
        "buffers": [{"byteLength": len(buffer_data)}]
    }

    json_str = json.dumps(gltf, separators=(',', ':'))
    json_bytes = json_str.encode('utf-8')
    json_pad = (4 - len(json_bytes) % 4) % 4
    json_bytes += b' ' * json_pad

    bin_pad = (4 - len(buffer_data) % 4) % 4
    buffer_data += b'\x00' * bin_pad

    total_length = 12 + 8 + len(json_bytes) + 8 + len(buffer_data)

    with open(out_path, 'wb') as f:
        f.write(struct.pack('<4sII', b'glTF', 2, total_length))
        f.write(struct.pack('<I4s', len(json_bytes), b'JSON'))
        f.write(json_bytes)
        f.write(struct.pack('<I4s', len(buffer_data), b'BIN\x00'))
        f.write(buffer_data)

    size = os.path.getsize(out_path)
    print(f"  {os.path.basename(out_path)}: {num_verts} verts, {num_indices} indices, {size} bytes")


def gen_cube():
    s = 0.5
    positions = [
        -s,-s, s,  s,-s, s,  s, s, s, -s, s, s,
         s,-s,-s, -s,-s,-s, -s, s,-s,  s, s,-s,
        -s, s, s,  s, s, s,  s, s,-s, -s, s,-s,
        -s,-s,-s,  s,-s,-s,  s,-s, s, -s,-s, s,
         s,-s, s,  s,-s,-s,  s, s,-s,  s, s, s,
        -s,-s,-s, -s,-s, s, -s, s, s, -s, s,-s,
    ]
    normals = [
        0,0,1, 0,0,1, 0,0,1, 0,0,1,
        0,0,-1, 0,0,-1, 0,0,-1, 0,0,-1,
        0,1,0, 0,1,0, 0,1,0, 0,1,0,
        0,-1,0, 0,-1,0, 0,-1,0, 0,-1,0,
        1,0,0, 1,0,0, 1,0,0, 1,0,0,
        -1,0,0, -1,0,0, -1,0,0, -1,0,0,
    ]
    texcoords = [0,1,1,1,1,0,0,0]*6
    indices = [
        0,1,2,0,2,3, 4,5,6,4,6,7, 8,9,10,8,10,11,
        12,13,14,12,14,15, 16,17,18,16,18,19, 20,21,22,20,22,23,
    ]
    return positions, normals, texcoords, indices


def gen_sphere(slices=16, stacks=14):
    r = 0.5
    positions, normals, texcoords = [], [], []
    for st in range(stacks + 1):
        phi = math.pi * st / stacks
        sp, cp = math.sin(phi), math.cos(phi)
        for sl in range(slices + 1):
            theta = 2 * math.pi * sl / slices
            st2, ct = math.sin(theta), math.cos(theta)
            nx, ny, nz = sp * ct, cp, sp * st2
            positions += [r * nx, r * ny, r * nz]
            normals += [nx, ny, nz]
            texcoords += [sl / slices, st / stacks]

    indices = []
    for st in range(stacks):
        for sl in range(slices):
            a = st * (slices + 1) + sl
            b = a + slices + 1
            indices += [a, b, a + 1, a + 1, b, b + 1]
    return positions, normals, texcoords, indices


def gen_cylinder(sides=16):
    r, hh = 0.5, 0.5
    positions, normals, texcoords = [], [], []

    # Side
    for i in range(sides + 1):
        a = 2 * math.pi * i / sides
        c, s = math.cos(a), math.sin(a)
        positions += [r*c, -hh, r*s]
        normals += [c, 0, s]
        texcoords += [i/sides, 1]
        positions += [r*c, hh, r*s]
        normals += [c, 0, s]
        texcoords += [i/sides, 0]

    # Top cap
    tc = len(positions) // 3
    positions += [0, hh, 0]; normals += [0, 1, 0]; texcoords += [0.5, 0.5]
    for i in range(sides + 1):
        a = 2 * math.pi * i / sides
        c, s = math.cos(a), math.sin(a)
        positions += [r*c, hh, r*s]; normals += [0, 1, 0]
        texcoords += [c*0.5+0.5, s*0.5+0.5]

    # Bottom cap
    bc = len(positions) // 3
    positions += [0, -hh, 0]; normals += [0, -1, 0]; texcoords += [0.5, 0.5]
    for i in range(sides + 1):
        a = 2 * math.pi * i / sides
        c, s = math.cos(a), math.sin(a)
        positions += [r*c, -hh, r*s]; normals += [0, -1, 0]
        texcoords += [c*0.5+0.5, s*0.5+0.5]

    indices = []
    for i in range(sides):
        bl = i * 2; tl = bl + 1; br = bl + 2; tr = bl + 3
        indices += [bl, br, tl, tl, br, tr]
    for i in range(sides):
        indices += [tc, tc+1+i, tc+1+i+1]
    for i in range(sides):
        indices += [bc, bc+1+i+1, bc+1+i]
    return positions, normals, texcoords, indices


def gen_cone(sides=16):
    r, hh = 0.5, 0.5
    positions, normals, texcoords = [], [], []

    slope = math.atan2(r, 2 * hh)
    ny = math.sin(slope)
    nr = math.cos(slope)

    for i in range(sides):
        a0 = 2 * math.pi * i / sides
        a1 = 2 * math.pi * (i + 1) / sides
        amid = (a0 + a1) / 2
        nxm = nr * math.cos(amid)
        nzm = nr * math.sin(amid)
        positions += [0, hh, 0]
        normals += [nxm, ny, nzm]
        texcoords += [(i + 0.5) / sides, 0]
        positions += [r*math.cos(a0), -hh, r*math.sin(a0)]
        normals += [nxm, ny, nzm]
        texcoords += [i / sides, 1]
        positions += [r*math.cos(a1), -hh, r*math.sin(a1)]
        normals += [nxm, ny, nzm]
        texcoords += [(i + 1) / sides, 1]

    # Base cap
    bc = len(positions) // 3
    positions += [0, -hh, 0]; normals += [0, -1, 0]; texcoords += [0.5, 0.5]
    for i in range(sides + 1):
        a = 2 * math.pi * i / sides
        c, s = math.cos(a), math.sin(a)
        positions += [r*c, -hh, r*s]; normals += [0, -1, 0]
        texcoords += [c*0.5+0.5, s*0.5+0.5]

    indices = []
    for i in range(sides):
        b = i * 3
        indices += [b, b+1, b+2]
    for i in range(sides):
        indices += [bc, bc+1+i+1, bc+1+i]
    return positions, normals, texcoords, indices


def gen_pyramid():
    s, apex = 0.5, 0.5
    by = -s
    corners = [(-s, by, s), (s, by, s), (s, by, -s), (-s, by, -s)]
    faces = [(0,1), (1,2), (2,3), (3,0)]

    positions, normals, texcoords = [], [], []

    for ci0, ci1 in faces:
        x0, _, z0 = corners[ci0]
        x1, _, z1 = corners[ci1]
        e1 = (x1-x0, 0, z1-z0)
        e2 = (-x0, apex-by, -z0)
        nx = e1[1]*e2[2] - e1[2]*e2[1]
        ny = e1[2]*e2[0] - e1[0]*e2[2]
        nz = e1[0]*e2[1] - e1[1]*e2[0]
        nl = math.sqrt(nx*nx + ny*ny + nz*nz)
        if nl > 0: nx /= nl; ny /= nl; nz /= nl

        positions += [0, apex, 0]; normals += [nx, ny, nz]; texcoords += [0.5, 0]
        positions += [x0, by, z0]; normals += [nx, ny, nz]; texcoords += [0, 1]
        positions += [x1, by, z1]; normals += [nx, ny, nz]; texcoords += [1, 1]

    # Base (2 triangles)
    bfl, bfr, bbr, bbl = corners
    for (px, py, pz), u, v in [
        (bfl, 0, 1), (bbl, 0, 0), (bfr, 1, 1),
        (bfr, 1, 1), (bbl, 0, 0), (bbr, 1, 0),
    ]:
        positions += [px, py, pz]; normals += [0, -1, 0]; texcoords += [u, v]

    indices = list(range(len(positions) // 3))
    return positions, normals, texcoords, indices


def main():
    out_dir = os.path.join(os.path.dirname(__file__), '..', 'assets', 'shapes')
    os.makedirs(out_dir, exist_ok=True)

    shapes = {
        'cube': gen_cube,
        'sphere': gen_sphere,
        'cylinder': gen_cylinder,
        'cone': gen_cone,
        'pyramid': gen_pyramid,
    }

    print("Generating shape GLBs:")
    for name, gen_fn in shapes.items():
        positions, normals, texcoords, indices = gen_fn()
        out_path = os.path.join(out_dir, f'{name}.glb')
        write_glb(out_path, positions, normals, texcoords, indices)


if __name__ == '__main__':
    main()
