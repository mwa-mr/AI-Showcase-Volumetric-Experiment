#pragma once

#include <numbers>

namespace sample {
    constexpr VaColor4f HSVtoRGB(float h, float s, float v) {
        float r = 0;
        float g = 0;
        float b = 0;
        int i = static_cast<int>(h * 6);
        float f = h * 6 - i;
        float p = v * (1 - s);
        float q = v * (1 - f * s);
        float t = v * (1 - (1 - f) * s);
        switch (i % 6) {
        case 0:
            r = v, g = t, b = p;
            break;
        case 1:
            r = q, g = v, b = p;
            break;
        case 2:
            r = p, g = v, b = t;
            break;
        case 3:
            r = p, g = q, b = v;
            break;
        case 4:
            r = t, g = p, b = v;
            break;
        case 5:
            r = v, g = p, b = q;
            break;
        }

        return VaColor4f{r, g, b, 1};
    }

    inline float PositiveSinWave(float time, float period) {
        const float frequency = 1.f / period;
        return static_cast<float>(0.5 * (1 + sin(2 * std::numbers::pi * frequency * time)));
    }
} // namespace sample
