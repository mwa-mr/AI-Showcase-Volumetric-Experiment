#pragma once

namespace va {

    /// <summary>
    /// Type is copyable and movable, typically used as data value struct.
    /// The type must not own non-copyable fields for resources lifecycle.
    /// </summary>
    class Copyable {
    public:
        virtual ~Copyable() = default;
        Copyable(Copyable&&) = default;
        Copyable& operator=(Copyable&&) = default;
        Copyable(const Copyable&) = default;
        Copyable& operator=(const Copyable&) = default;

    protected:
        Copyable() = default;
    };

    /// <summary>
    /// Type is not movable nor copiable, typically used as polymorphic base classes.
    /// The type may own the lifecycle of non-copyable non-movable resources for RAII.
    /// </summary>
    class NonMovable {
    public:
        virtual ~NonMovable() = default;

    protected:
        NonMovable() = default;

    private:
        NonMovable(const NonMovable&) = delete;
        NonMovable& operator=(const NonMovable&) = delete;
        NonMovable(NonMovable&&) = delete;
        NonMovable& operator=(NonMovable&&) = delete;
    };

} // namespace va
