"""Generate a template.glb for procedural mesh editing.

The template contains a unit cube (24 vertices, 36 indices) with positions,
normals, UVs, and a PBR material named 'mat'. It is used as a base for
MeshResource.WriteMeshBuffers which replaces the geometry at runtime.

Uses a full cube rather than a minimal triangle to ensure the native mesh
resize infrastructure works reliably (matches the structure of Simple.gltf
and BoxTextured.glb which are used by working SDK samples).

Uses raw glTF/GLB construction to ensure all required attributes are present.
"""

import json
import struct
import os


def main():
    # Unit cube: 6 faces × 4 verts each = 24 vertices, 36 indices
    s = 0.5
    # fmt: off
    positions = [
        # Front face (z = +s)
        -s, -s,  s,   s, -s,  s,   s,  s,  s,  -s,  s,  s,
        # Back face (z = -s)
         s, -s, -s,  -s, -s, -s,  -s,  s, -s,   s,  s, -s,
        # Top face (y = +s)
        -s,  s,  s,   s,  s,  s,   s,  s, -s,  -s,  s, -s,
        # Bottom face (y = -s)
        -s, -s, -s,   s, -s, -s,   s, -s,  s,  -s, -s,  s,
        # Right face (x = +s)
         s, -s,  s,   s, -s, -s,   s,  s, -s,   s,  s,  s,
        # Left face (x = -s)
        -s, -s, -s,  -s, -s,  s,  -s,  s,  s,  -s,  s, -s,
    ]
    normals = [
         0, 0, 1,   0, 0, 1,   0, 0, 1,   0, 0, 1,
         0, 0,-1,   0, 0,-1,   0, 0,-1,   0, 0,-1,
         0, 1, 0,   0, 1, 0,   0, 1, 0,   0, 1, 0,
         0,-1, 0,   0,-1, 0,   0,-1, 0,   0,-1, 0,
         1, 0, 0,   1, 0, 0,   1, 0, 0,   1, 0, 0,
        -1, 0, 0,  -1, 0, 0,  -1, 0, 0,  -1, 0, 0,
    ]
    texcoords = [
        0, 1,  1, 1,  1, 0,  0, 0,
        0, 1,  1, 1,  1, 0,  0, 0,
        0, 1,  1, 1,  1, 0,  0, 0,
        0, 1,  1, 1,  1, 0,  0, 0,
        0, 1,  1, 1,  1, 0,  0, 0,
        0, 1,  1, 1,  1, 0,  0, 0,
    ]
    indices = [
         0,  1,  2,   0,  2,  3,
         4,  5,  6,   4,  6,  7,
         8,  9, 10,   8, 10, 11,
        12, 13, 14,  12, 14, 15,
        16, 17, 18,  16, 18, 19,
        20, 21, 22,  20, 22, 23,
    ]
    # fmt: on

    # Pack binary buffer: indices (uint32) + positions (float32) + normals (float32) + texcoords (float32)
    idx_bytes = struct.pack(f'<{len(indices)}I', *indices)
    pos_bytes = struct.pack(f'<{len(positions)}f', *positions)
    norm_bytes = struct.pack(f'<{len(normals)}f', *normals)
    uv_bytes = struct.pack(f'<{len(texcoords)}f', *texcoords)

    # Pad each section to 4-byte alignment
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

    num_verts = len(positions) // 3
    num_indices = len(indices)

    gltf = {
        "asset": {"version": "2.0", "generator": "generate_template.py"},
        "scene": 0,
        "scenes": [{"nodes": [0]}],
        "nodes": [{"name": "root", "mesh": 0}],
        "meshes": [{
            "name": "template_mesh",
            "primitives": [{
                "attributes": {
                    "POSITION": 1,
                    "NORMAL": 2,
                    "TEXCOORD_0": 3
                },
                "indices": 0,
                "material": 0,
                "mode": 4
            }]
        }],
        "materials": [{
            "name": "mat",
            "pbrMetallicRoughness": {
                "baseColorFactor": [1.0, 1.0, 1.0, 1.0],
                "metallicFactor": 0.0,
                "roughnessFactor": 1.0
            },
            "doubleSided": False
        }],
        "accessors": [
            {  # 0: indices
                "bufferView": 0,
                "componentType": 5125,  # UNSIGNED_INT
                "count": num_indices,
                "type": "SCALAR",
                "max": [num_verts - 1],
                "min": [0]
            },
            {  # 1: positions
                "bufferView": 1,
                "componentType": 5126,  # FLOAT
                "count": num_verts,
                "type": "VEC3",
                "max": [0.5, 0.5, 0.5],
                "min": [-0.5, -0.5, -0.5]
            },
            {  # 2: normals
                "bufferView": 2,
                "componentType": 5126,
                "count": num_verts,
                "type": "VEC3",
                "max": [1.0, 1.0, 1.0],
                "min": [-1.0, -1.0, -1.0]
            },
            {  # 3: texcoords
                "bufferView": 3,
                "componentType": 5126,
                "count": num_verts,
                "type": "VEC2",
                "max": [1.0, 1.0],
                "min": [0.0, 0.0]
            }
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
    # Pad JSON to 4-byte alignment with spaces
    json_pad = (4 - len(json_bytes) % 4) % 4
    json_bytes += b' ' * json_pad

    # Pad binary to 4-byte alignment with zeros
    bin_pad = (4 - len(buffer_data) % 4) % 4
    buffer_data += b'\x00' * bin_pad

    total_length = 12 + 8 + len(json_bytes) + 8 + len(buffer_data)

    out_dir = os.path.join(os.path.dirname(__file__), '..', 'assets', 'shapes')
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, 'template.glb')

    with open(out_path, 'wb') as f:
        # GLB header
        f.write(struct.pack('<4sII', b'glTF', 2, total_length))
        # JSON chunk
        f.write(struct.pack('<I4s', len(json_bytes), b'JSON'))
        f.write(json_bytes)
        # Binary chunk
        f.write(struct.pack('<I4s', len(buffer_data), b'BIN\x00'))
        f.write(buffer_data)

    size = os.path.getsize(out_path)
    print(f"Generated {out_path} ({size} bytes)")

    # Verify
    with open(out_path, 'rb') as f:
        magic = f.read(4)
        ver = struct.unpack('<I', f.read(4))[0]
        length = struct.unpack('<I', f.read(4))[0]
        print(f"Verification: magic={magic} version={ver} length={length}")
        cl = struct.unpack('<I', f.read(4))[0]
        ct = f.read(4)
        check_gltf = json.loads(f.read(cl))
        attrs = check_gltf['meshes'][0]['primitives'][0]['attributes']
        mat_name = check_gltf['materials'][0]['name']
        print(f"Attributes: {attrs}")
        print(f"Material name: {mat_name}")


if __name__ == '__main__':
    main()
