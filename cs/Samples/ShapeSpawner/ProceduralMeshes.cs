namespace CsShapeSpawner;

internal static class ProceduralMeshes
{
    public static (float[] positions, float[] normals, float[] texcoords, uint[] indices) GenerateCube()
    {
        float s = 0.5f;
        // 6 faces, 4 verts each = 24 verts, 36 indices
        float[] positions =
        [
            // Front face (z = +s)
            -s, -s,  s,   s, -s,  s,   s,  s,  s,  -s,  s,  s,
            // Back face (z = -s)
             s, -s, -s,  -s, -s, -s,  -s,  s, -s,   s,  s, -s,
            // Top face (y = +s)
            -s,  s,  s,   s,  s,  s,   s,  s, -s,  -s,  s, -s,
            // Bottom face (y = -s)
            -s, -s, -s,   s, -s, -s,   s, -s,  s,  -s, -s,  s,
            // Right face (x = +s)
             s, -s,  s,   s, -s, -s,   s,  s, -s,   s,  s,  s,
            // Left face (x = -s)
            -s, -s, -s,  -s, -s,  s,  -s,  s,  s,  -s,  s, -s,
        ];

        float[] normals =
        [
            0, 0, 1,  0, 0, 1,  0, 0, 1,  0, 0, 1,
            0, 0,-1,  0, 0,-1,  0, 0,-1,  0, 0,-1,
            0, 1, 0,  0, 1, 0,  0, 1, 0,  0, 1, 0,
            0,-1, 0,  0,-1, 0,  0,-1, 0,  0,-1, 0,
            1, 0, 0,  1, 0, 0,  1, 0, 0,  1, 0, 0,
           -1, 0, 0, -1, 0, 0, -1, 0, 0, -1, 0, 0,
        ];

        float[] texcoords =
        [
            0, 1,  1, 1,  1, 0,  0, 0,
            0, 1,  1, 1,  1, 0,  0, 0,
            0, 1,  1, 1,  1, 0,  0, 0,
            0, 1,  1, 1,  1, 0,  0, 0,
            0, 1,  1, 1,  1, 0,  0, 0,
            0, 1,  1, 1,  1, 0,  0, 0,
        ];

        uint[] indices =
        [
             0, 1, 2,  0, 2, 3,
             4, 5, 6,  4, 6, 7,
             8, 9,10,  8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        ];

        return (positions, normals, texcoords, indices);
    }

    public static (float[] positions, float[] normals, float[] texcoords, uint[] indices) GenerateSphere(int slices = 16, int stacks = 14)
    {
        float radius = 0.5f;
        int vertCount = (stacks + 1) * (slices + 1);
        float[] positions = new float[vertCount * 3];
        float[] normals = new float[vertCount * 3];
        float[] texcoords = new float[vertCount * 2];

        int vi = 0, ti = 0;
        for (int st = 0; st <= stacks; st++)
        {
            float phi = MathF.PI * st / stacks;
            float sinPhi = MathF.Sin(phi);
            float cosPhi = MathF.Cos(phi);

            for (int sl = 0; sl <= slices; sl++)
            {
                float theta = 2f * MathF.PI * sl / slices;
                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                float nx = sinPhi * cosTheta;
                float ny = cosPhi;
                float nz = sinPhi * sinTheta;

                positions[vi] = radius * nx;
                positions[vi + 1] = radius * ny;
                positions[vi + 2] = radius * nz;
                normals[vi] = nx;
                normals[vi + 1] = ny;
                normals[vi + 2] = nz;
                vi += 3;

                texcoords[ti] = (float)sl / slices;
                texcoords[ti + 1] = (float)st / stacks;
                ti += 2;
            }
        }

        int indexCount = stacks * slices * 6;
        uint[] indices = new uint[indexCount];
        int idx = 0;
        for (int st = 0; st < stacks; st++)
        {
            for (int sl = 0; sl < slices; sl++)
            {
                uint a = (uint)(st * (slices + 1) + sl);
                uint b = a + (uint)(slices + 1);
                indices[idx++] = a;
                indices[idx++] = b;
                indices[idx++] = a + 1;
                indices[idx++] = a + 1;
                indices[idx++] = b;
                indices[idx++] = b + 1;
            }
        }

        return (positions, normals, texcoords, indices);
    }

    public static (float[] positions, float[] normals, float[] texcoords, uint[] indices) GenerateCylinder(int sides = 16)
    {
        float radius = 0.5f;
        float halfH = 0.5f;

        // Side: (sides+1)*2 verts, Top cap: sides+1 verts (+center), Bottom cap: sides+1 verts (+center)
        int sideVerts = (sides + 1) * 2;
        int capVerts = sides + 2; // center + ring
        int totalVerts = sideVerts + capVerts * 2;

        float[] positions = new float[totalVerts * 3];
        float[] normals = new float[totalVerts * 3];
        float[] texcoords = new float[totalVerts * 2];

        int vi = 0, ti = 0;

        // Side vertices
        for (int i = 0; i <= sides; i++)
        {
            float angle = 2f * MathF.PI * i / sides;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            // Bottom
            positions[vi] = radius * cos; positions[vi + 1] = -halfH; positions[vi + 2] = radius * sin;
            normals[vi] = cos; normals[vi + 1] = 0; normals[vi + 2] = sin;
            vi += 3;
            texcoords[ti] = (float)i / sides; texcoords[ti + 1] = 1;
            ti += 2;

            // Top
            positions[vi] = radius * cos; positions[vi + 1] = halfH; positions[vi + 2] = radius * sin;
            normals[vi] = cos; normals[vi + 1] = 0; normals[vi + 2] = sin;
            vi += 3;
            texcoords[ti] = (float)i / sides; texcoords[ti + 1] = 0;
            ti += 2;
        }

        // Top cap
        int topCenterIdx = vi / 3;
        positions[vi] = 0; positions[vi + 1] = halfH; positions[vi + 2] = 0;
        normals[vi] = 0; normals[vi + 1] = 1; normals[vi + 2] = 0;
        vi += 3;
        texcoords[ti] = 0.5f; texcoords[ti + 1] = 0.5f;
        ti += 2;

        for (int i = 0; i <= sides; i++)
        {
            float angle = 2f * MathF.PI * i / sides;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            positions[vi] = radius * cos; positions[vi + 1] = halfH; positions[vi + 2] = radius * sin;
            normals[vi] = 0; normals[vi + 1] = 1; normals[vi + 2] = 0;
            vi += 3;
            texcoords[ti] = cos * 0.5f + 0.5f; texcoords[ti + 1] = sin * 0.5f + 0.5f;
            ti += 2;
        }

        // Bottom cap
        int botCenterIdx = vi / 3;
        positions[vi] = 0; positions[vi + 1] = -halfH; positions[vi + 2] = 0;
        normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0;
        vi += 3;
        texcoords[ti] = 0.5f; texcoords[ti + 1] = 0.5f;
        ti += 2;

        for (int i = 0; i <= sides; i++)
        {
            float angle = 2f * MathF.PI * i / sides;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            positions[vi] = radius * cos; positions[vi + 1] = -halfH; positions[vi + 2] = radius * sin;
            normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0;
            vi += 3;
            texcoords[ti] = cos * 0.5f + 0.5f; texcoords[ti + 1] = sin * 0.5f + 0.5f;
            ti += 2;
        }

        // Indices
        int sideIndices = sides * 6;
        int capIndices = sides * 3 * 2;
        uint[] indices = new uint[sideIndices + capIndices];
        int idx = 0;

        // Side
        for (int i = 0; i < sides; i++)
        {
            uint bl = (uint)(i * 2);
            uint tl = bl + 1;
            uint br = bl + 2;
            uint tr = bl + 3;
            indices[idx++] = bl; indices[idx++] = br; indices[idx++] = tl;
            indices[idx++] = tl; indices[idx++] = br; indices[idx++] = tr;
        }

        // Top cap
        for (int i = 0; i < sides; i++)
        {
            indices[idx++] = (uint)topCenterIdx;
            indices[idx++] = (uint)(topCenterIdx + 1 + i);
            indices[idx++] = (uint)(topCenterIdx + 1 + i + 1);
        }

        // Bottom cap
        for (int i = 0; i < sides; i++)
        {
            indices[idx++] = (uint)botCenterIdx;
            indices[idx++] = (uint)(botCenterIdx + 1 + i + 1);
            indices[idx++] = (uint)(botCenterIdx + 1 + i);
        }

        return (positions, normals, texcoords, indices);
    }

    public static (float[] positions, float[] normals, float[] texcoords, uint[] indices) GenerateCone(int sides = 16)
    {
        float radius = 0.5f;
        float halfH = 0.5f;

        // Apex repeated per side triangle + base ring + base center
        int sideVerts = sides * 3; // each triangle: apex + 2 base
        int baseVerts = sides + 2; // center + ring
        int totalVerts = sideVerts + baseVerts;

        float[] positions = new float[totalVerts * 3];
        float[] normals = new float[totalVerts * 3];
        float[] texcoords = new float[totalVerts * 2];

        int vi = 0, ti = 0;

        // Side triangles - each has its own apex + base verts for proper normals
        float slopeAngle = MathF.Atan2(radius, 2f * halfH);
        float ny = MathF.Sin(slopeAngle);
        float nr = MathF.Cos(slopeAngle);

        for (int i = 0; i < sides; i++)
        {
            float a0 = 2f * MathF.PI * i / sides;
            float a1 = 2f * MathF.PI * (i + 1) / sides;
            float aMid = (a0 + a1) / 2f;

            float nxMid = nr * MathF.Cos(aMid);
            float nzMid = nr * MathF.Sin(aMid);

            // Apex
            positions[vi] = 0; positions[vi + 1] = halfH; positions[vi + 2] = 0;
            normals[vi] = nxMid; normals[vi + 1] = ny; normals[vi + 2] = nzMid;
            vi += 3;
            texcoords[ti] = (float)(i + 0.5f) / sides; texcoords[ti + 1] = 0;
            ti += 2;

            // Base vertex 0
            positions[vi] = radius * MathF.Cos(a0); positions[vi + 1] = -halfH; positions[vi + 2] = radius * MathF.Sin(a0);
            normals[vi] = nxMid; normals[vi + 1] = ny; normals[vi + 2] = nzMid;
            vi += 3;
            texcoords[ti] = (float)i / sides; texcoords[ti + 1] = 1;
            ti += 2;

            // Base vertex 1
            positions[vi] = radius * MathF.Cos(a1); positions[vi + 1] = -halfH; positions[vi + 2] = radius * MathF.Sin(a1);
            normals[vi] = nxMid; normals[vi + 1] = ny; normals[vi + 2] = nzMid;
            vi += 3;
            texcoords[ti] = (float)(i + 1) / sides; texcoords[ti + 1] = 1;
            ti += 2;
        }

        // Base cap
        int baseCenterIdx = vi / 3;
        positions[vi] = 0; positions[vi + 1] = -halfH; positions[vi + 2] = 0;
        normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0;
        vi += 3;
        texcoords[ti] = 0.5f; texcoords[ti + 1] = 0.5f;
        ti += 2;

        for (int i = 0; i <= sides; i++)
        {
            float angle = 2f * MathF.PI * i / sides;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            positions[vi] = radius * cos; positions[vi + 1] = -halfH; positions[vi + 2] = radius * sin;
            normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0;
            vi += 3;
            texcoords[ti] = cos * 0.5f + 0.5f; texcoords[ti + 1] = sin * 0.5f + 0.5f;
            ti += 2;
        }

        // Indices
        uint[] indices = new uint[sides * 3 + sides * 3];
        int idx = 0;

        // Side triangles
        for (int i = 0; i < sides; i++)
        {
            uint baseIdx = (uint)(i * 3);
            indices[idx++] = baseIdx;
            indices[idx++] = baseIdx + 1;
            indices[idx++] = baseIdx + 2;
        }

        // Base cap
        for (int i = 0; i < sides; i++)
        {
            indices[idx++] = (uint)baseCenterIdx;
            indices[idx++] = (uint)(baseCenterIdx + 1 + i + 1);
            indices[idx++] = (uint)(baseCenterIdx + 1 + i);
        }

        return (positions, normals, texcoords, indices);
    }

    public static (float[] positions, float[] normals, float[] texcoords, uint[] indices) GeneratePyramid()
    {
        float s = 0.5f;
        float apex = 0.5f;

        // 4 side faces × 3 verts + 2 base triangles × 3 verts = 18 verts (unique per face for normals)
        // Base: 4 corners → 2 triangles
        float[] positions = new float[18 * 3];
        float[] normals = new float[18 * 3];
        float[] texcoords = new float[18 * 2];

        int vi = 0, ti = 0;

        // Base corners
        float bfl_x = -s, bfl_z = s;   // front-left
        float bfr_x = s, bfr_z = s;    // front-right
        float bbr_x = s, bbr_z = -s;   // back-right
        float bbl_x = -s, bbl_z = -s;  // back-left
        float by = -s;

        // Side faces: front, right, back, left
        (float x0, float z0, float x1, float z1)[] sides =
        [
            (bfl_x, bfl_z, bfr_x, bfr_z),
            (bfr_x, bfr_z, bbr_x, bbr_z),
            (bbr_x, bbr_z, bbl_x, bbl_z),
            (bbl_x, bbl_z, bfl_x, bfl_z),
        ];

        foreach (var (x0, z0, x1, z1) in sides)
        {
            // Calculate face normal
            float mx = (x0 + x1) / 2f;
            float mz = (z0 + z1) / 2f;
            float len = MathF.Sqrt(mx * mx + apex * apex + mz * mz);
            // Edge vectors for cross product
            float e1x = x1 - x0, e1y = 0f, e1z = z1 - z0;
            float e2x = 0 - x0, e2y = apex - by, e2z = 0 - z0;
            float nx = e1y * e2z - e1z * e2y;
            float ny = e1z * e2x - e1x * e2z;
            float nz = e1x * e2y - e1y * e2x;
            float nLen = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
            if (nLen > 0) { nx /= nLen; ny /= nLen; nz /= nLen; }

            // Apex
            positions[vi] = 0; positions[vi + 1] = apex; positions[vi + 2] = 0;
            normals[vi] = nx; normals[vi + 1] = ny; normals[vi + 2] = nz;
            vi += 3;
            texcoords[ti] = 0.5f; texcoords[ti + 1] = 0;
            ti += 2;

            // Base left
            positions[vi] = x0; positions[vi + 1] = by; positions[vi + 2] = z0;
            normals[vi] = nx; normals[vi + 1] = ny; normals[vi + 2] = nz;
            vi += 3;
            texcoords[ti] = 0; texcoords[ti + 1] = 1;
            ti += 2;

            // Base right
            positions[vi] = x1; positions[vi + 1] = by; positions[vi + 2] = z1;
            normals[vi] = nx; normals[vi + 1] = ny; normals[vi + 2] = nz;
            vi += 3;
            texcoords[ti] = 1; texcoords[ti + 1] = 1;
            ti += 2;
        }

        // Base (2 triangles, normal facing down)
        // Triangle 1: front-left, back-left, front-right
        positions[vi] = bfl_x; positions[vi + 1] = by; positions[vi + 2] = bfl_z;
        normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0; vi += 3;
        texcoords[ti] = 0; texcoords[ti + 1] = 1; ti += 2;

        positions[vi] = bbl_x; positions[vi + 1] = by; positions[vi + 2] = bbl_z;
        normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0; vi += 3;
        texcoords[ti] = 0; texcoords[ti + 1] = 0; ti += 2;

        positions[vi] = bfr_x; positions[vi + 1] = by; positions[vi + 2] = bfr_z;
        normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0; vi += 3;
        texcoords[ti] = 1; texcoords[ti + 1] = 1; ti += 2;

        // Triangle 2: front-right, back-left, back-right
        positions[vi] = bfr_x; positions[vi + 1] = by; positions[vi + 2] = bfr_z;
        normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0; vi += 3;
        texcoords[ti] = 1; texcoords[ti + 1] = 1; ti += 2;

        positions[vi] = bbl_x; positions[vi + 1] = by; positions[vi + 2] = bbl_z;
        normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0; vi += 3;
        texcoords[ti] = 0; texcoords[ti + 1] = 0; ti += 2;

        positions[vi] = bbr_x; positions[vi + 1] = by; positions[vi + 2] = bbr_z;
        normals[vi] = 0; normals[vi + 1] = -1; normals[vi + 2] = 0; vi += 3;
        texcoords[ti] = 1; texcoords[ti + 1] = 0; ti += 2;

        uint[] indices = new uint[18];
        for (uint i = 0; i < 18; i++) indices[i] = i;

        return (positions, normals, texcoords, indices);
    }

    public static (float[] positions, float[] normals, float[] texcoords, uint[] indices) GenerateWireframeCube(float thickness, int cylinderSides = 8)
    {
        float s = 0.5f;
        float radius = thickness / 2f;

        // 8 corners of the cube
        (float x, float y, float z)[] corners =
        [
            (-s, -s, -s), ( s, -s, -s), ( s,  s, -s), (-s,  s, -s),
            (-s, -s,  s), ( s, -s,  s), ( s,  s,  s), (-s,  s,  s),
        ];

        // 12 edges
        (int a, int b)[] edges =
        [
            (0,1),(1,2),(2,3),(3,0),
            (4,5),(5,6),(6,7),(7,4),
            (0,4),(1,5),(2,6),(3,7),
        ];

        // Each edge cylinder: (cylinderSides+1)*2 side verts + 2 cap centers + 2*(cylinderSides+1) cap ring verts
        int sideVerts = (cylinderSides + 1) * 2;
        int capVerts = (cylinderSides + 2); // center + ring
        int vertsPerEdge = sideVerts + capVerts * 2;
        int sideIndices = cylinderSides * 6;
        int capIndices = cylinderSides * 3 * 2;
        int indicesPerEdge = sideIndices + capIndices;

        int totalVerts = edges.Length * vertsPerEdge;
        int totalIndices = edges.Length * indicesPerEdge;
        float[] positions = new float[totalVerts * 3];
        float[] normals = new float[totalVerts * 3];
        float[] texcoords = new float[totalVerts * 2];
        uint[] indices = new uint[totalIndices];

        int vi = 0, ti = 0, idx = 0;

        for (int e = 0; e < edges.Length; e++)
        {
            var (ai, bi) = edges[e];
            var (ax, ay, az) = corners[ai];
            var (bx, by, bz) = corners[bi];

            // Direction along the edge
            float dx = bx - ax, dy = by - ay, dz = bz - az;
            float len = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
            dx /= len; dy /= len; dz /= len;

            // Two perpendicular vectors to the edge direction
            float ux, uy, uz;
            if (MathF.Abs(dx) < 0.9f)
            {
                ux = 0; uy = -dz; uz = dy;
            }
            else
            {
                ux = dz; uy = 0; uz = -dx;
            }
            float uLen = MathF.Sqrt(ux * ux + uy * uy + uz * uz);
            ux /= uLen; uy /= uLen; uz /= uLen;

            float vx = dy * uz - dz * uy;
            float vy = dz * ux - dx * uz;
            float vz = dx * uy - dy * ux;

            uint baseVert = (uint)(vi / 3);

            // Side vertices: ring at start and ring at end
            for (int i = 0; i <= cylinderSides; i++)
            {
                float angle = 2f * MathF.PI * i / cylinderSides;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);

                float ox = (ux * cos + vx * sin) * radius;
                float oy = (uy * cos + vy * sin) * radius;
                float oz = (uz * cos + vz * sin) * radius;

                float nx = ux * cos + vx * sin;
                float ny = uy * cos + vy * sin;
                float nz = uz * cos + vz * sin;

                // Start ring vertex
                positions[vi] = ax + ox; positions[vi + 1] = ay + oy; positions[vi + 2] = az + oz;
                normals[vi] = nx; normals[vi + 1] = ny; normals[vi + 2] = nz;
                vi += 3;
                texcoords[ti] = (float)i / cylinderSides; texcoords[ti + 1] = 1;
                ti += 2;

                // End ring vertex
                positions[vi] = bx + ox; positions[vi + 1] = by + oy; positions[vi + 2] = bz + oz;
                normals[vi] = nx; normals[vi + 1] = ny; normals[vi + 2] = nz;
                vi += 3;
                texcoords[ti] = (float)i / cylinderSides; texcoords[ti + 1] = 0;
                ti += 2;
            }

            // Start cap (center + ring, normal = -edge direction)
            uint startCapCenter = (uint)(vi / 3);
            positions[vi] = ax; positions[vi + 1] = ay; positions[vi + 2] = az;
            normals[vi] = -dx; normals[vi + 1] = -dy; normals[vi + 2] = -dz;
            vi += 3;
            texcoords[ti] = 0.5f; texcoords[ti + 1] = 0.5f;
            ti += 2;

            for (int i = 0; i <= cylinderSides; i++)
            {
                float angle = 2f * MathF.PI * i / cylinderSides;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);
                float ox = (ux * cos + vx * sin) * radius;
                float oy = (uy * cos + vy * sin) * radius;
                float oz = (uz * cos + vz * sin) * radius;
                positions[vi] = ax + ox; positions[vi + 1] = ay + oy; positions[vi + 2] = az + oz;
                normals[vi] = -dx; normals[vi + 1] = -dy; normals[vi + 2] = -dz;
                vi += 3;
                texcoords[ti] = cos * 0.5f + 0.5f; texcoords[ti + 1] = sin * 0.5f + 0.5f;
                ti += 2;
            }

            // End cap (center + ring, normal = +edge direction)
            uint endCapCenter = (uint)(vi / 3);
            positions[vi] = bx; positions[vi + 1] = by; positions[vi + 2] = bz;
            normals[vi] = dx; normals[vi + 1] = dy; normals[vi + 2] = dz;
            vi += 3;
            texcoords[ti] = 0.5f; texcoords[ti + 1] = 0.5f;
            ti += 2;

            for (int i = 0; i <= cylinderSides; i++)
            {
                float angle = 2f * MathF.PI * i / cylinderSides;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);
                float ox = (ux * cos + vx * sin) * radius;
                float oy = (uy * cos + vy * sin) * radius;
                float oz = (uz * cos + vz * sin) * radius;
                positions[vi] = bx + ox; positions[vi + 1] = by + oy; positions[vi + 2] = bz + oz;
                normals[vi] = dx; normals[vi + 1] = dy; normals[vi + 2] = dz;
                vi += 3;
                texcoords[ti] = cos * 0.5f + 0.5f; texcoords[ti + 1] = sin * 0.5f + 0.5f;
                ti += 2;
            }

            // Side indices
            for (int i = 0; i < cylinderSides; i++)
            {
                uint bl = baseVert + (uint)(i * 2);
                uint tl = bl + 1;
                uint br = bl + 2;
                uint tr = bl + 3;
                indices[idx++] = bl; indices[idx++] = br; indices[idx++] = tl;
                indices[idx++] = tl; indices[idx++] = br; indices[idx++] = tr;
            }

            // Start cap indices (winding reversed — faces inward along edge)
            for (int i = 0; i < cylinderSides; i++)
            {
                indices[idx++] = startCapCenter;
                indices[idx++] = startCapCenter + 1 + (uint)i + 1;
                indices[idx++] = startCapCenter + 1 + (uint)i;
            }

            // End cap indices
            for (int i = 0; i < cylinderSides; i++)
            {
                indices[idx++] = endCapCenter;
                indices[idx++] = endCapCenter + 1 + (uint)i;
                indices[idx++] = endCapCenter + 1 + (uint)i + 1;
            }
        }

        return (positions, normals, texcoords, indices);
    }

    public static (float[] positions, float[] normals, float[] texcoords, uint[] indices) GenerateLabelPlane(float width, float height, float cornerRadius, int cornerSegments = 4)
    {
        // Generate a rounded rectangle in the XY plane centered at origin, facing +Z
        var posList = new List<float>();
        var normList = new List<float>();
        var uvList = new List<float>();
        var idxList = new List<uint>();

        float hw = width / 2f;
        float hh = height / 2f;
        float r = MathF.Min(cornerRadius, MathF.Min(hw, hh));

        // Center vertex
        uint centerIdx = 0;
        posList.AddRange([0, 0, 0]);
        normList.AddRange([0, 0, 1]);
        uvList.AddRange([0.5f, 0.5f]);

        // Build perimeter vertices going counter-clockwise
        var perimVerts = new List<(float x, float y)>();

        // 4 corners: top-right, top-left, bottom-left, bottom-right
        (float cx, float cy, float startAngle)[] cornerCenters =
        [
            ( hw - r,  hh - r, 0f),
            (-hw + r,  hh - r, MathF.PI / 2f),
            (-hw + r, -hh + r, MathF.PI),
            ( hw - r, -hh + r, 3f * MathF.PI / 2f),
        ];

        foreach (var (cx, cy, startAngle) in cornerCenters)
        {
            for (int i = 0; i <= cornerSegments; i++)
            {
                float angle = startAngle + (MathF.PI / 2f) * i / cornerSegments;
                float px = cx + r * MathF.Cos(angle);
                float py = cy + r * MathF.Sin(angle);
                perimVerts.Add((px, py));
            }
        }

        // Add perimeter vertices
        for (int i = 0; i < perimVerts.Count; i++)
        {
            var (px, py) = perimVerts[i];
            posList.AddRange([px, py, 0]);
            normList.AddRange([0, 0, 1]);
            uvList.AddRange([px / width + 0.5f, py / height + 0.5f]);
        }

        // Fan triangles from center
        uint perimStart = 1;
        uint perimCount = (uint)perimVerts.Count;
        for (uint i = 0; i < perimCount; i++)
        {
            uint next = (i + 1) % perimCount;
            idxList.Add(centerIdx);
            idxList.Add(perimStart + i);
            idxList.Add(perimStart + next);
        }

        return (posList.ToArray(), normList.ToArray(), uvList.ToArray(), idxList.ToArray());
    }

    public static (float[] positions, float[] normals, float[] texcoords, uint[] indices) GetShapeMesh(string shapeName)
    {
        return shapeName switch
        {
            "Cube" => GenerateCube(),
            "Sphere" => GenerateSphere(),
            "Cylinder" => GenerateCylinder(),
            "Cone" => GenerateCone(),
            "Pyramid" => GeneratePyramid(),
            _ => GenerateCube(),
        };
    }

    private static void AddQuad(uint[] indices, ref int idx, uint a, uint b, uint c, uint d)
    {
        indices[idx++] = a; indices[idx++] = b; indices[idx++] = c;
        indices[idx++] = a; indices[idx++] = c; indices[idx++] = d;
    }
}
