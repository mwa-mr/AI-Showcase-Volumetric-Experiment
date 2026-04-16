#ifndef VOLUMETRIC_PLATFORM_DEFINES_H_
#define VOLUMETRIC_PLATFORM_DEFINES_H_ 1

#ifdef __cplusplus
extern "C" {
#endif

/* Platform-specific calling convention macros.
 *
 * Platforms should define these so that clients call API functions
 * with the same calling conventions that the API implementation expects.
 *
 * VA_API_CALL - Placed after the return type in function declarations.
 *              Useful for MSVC-style calling convention syntax.
 * VA_API_PTR  - Placed between the '(' and '*' in function pointer types.
 *
 * Function declaration:  void VA_API_CALL vaFunction(void);
 * Function pointer type: typedef void (VA_API_PTR *PFN_vaFunction)(void);
 */
#if defined(_WIN32)
// On Windows, functions use the stdcall convention
#define VA_API_CALL __stdcall
#define VA_API_PTR VA_API_CALL
#else
#error "Unsupported platform"
#endif

#include <stddef.h>

#if !defined(VA_NO_STDINT_H)
#if defined(_MSC_VER) && (_MSC_VER < 1600)
typedef signed __int8 int8_t;
typedef unsigned __int8 uint8_t;
typedef signed __int16 int16_t;
typedef unsigned __int16 uint16_t;
typedef signed __int32 int32_t;
typedef unsigned __int32 uint32_t;
typedef signed __int64 int64_t;
typedef unsigned __int64 uint64_t;
#else
#include <stdint.h>
#endif
#endif // !defined( VA_NO_STDINT_H )

// VA_PTR_SIZE (in bytes)
#if (defined(__LP64__) || defined(_WIN64) || (defined(__x86_64__) && !defined(__ILP32__)) || defined(_M_X64) || defined(__ia64) || defined(_M_IA64) || defined(__aarch64__) || \
     defined(__powerpc64__))
#define VA_PTR_SIZE 8
#else
#define VA_PTR_SIZE 4
#endif

// Needed so we can use clang __has_feature portably.
#if !defined(VA_COMPILER_HAS_FEATURE)
#if defined(__clang__)
#define VA_COMPILER_HAS_FEATURE(x) __has_feature(x)
#else
#define VA_COMPILER_HAS_FEATURE(x) 0
#endif
#endif

// Identifies if the current compiler has C++11 support enabled.
// Does not by itself identify if any given C++11 feature is present.
#if !defined(VA_CPP11_ENABLED) && defined(__cplusplus)
#if defined(__GNUC__) && defined(__GXX_EXPERIMENTAL_CXX0X__)
#define VA_CPP11_ENABLED 1
#elif defined(_MSC_VER) && (_MSC_VER >= 1600)
#define VA_CPP11_ENABLED 1
#elif (__cplusplus >= 201103L) // 201103 is the first C++11 version.
#define VA_CPP11_ENABLED 1
#endif
#endif

// Identifies if the current compiler supports C++11 nullptr.
#if !defined(VA_CPP_NULLPTR_SUPPORTED)
#if defined(VA_CPP11_ENABLED) && ((defined(__clang__) && VA_COMPILER_HAS_FEATURE(cxx_nullptr)) || (defined(__GNUC__) && (((__GNUC__ * 1000) + __GNUC_MINOR__) >= 4006)) || \
                                  (defined(_MSC_VER) && (_MSC_VER >= 1600)) || (defined(__EDG_VERSION__) && (__EDG_VERSION__ >= 403)))
#define VA_CPP_NULLPTR_SUPPORTED 1
#endif
#endif

#if !defined(VA_CPP_NULLPTR_SUPPORTED)
#define VA_CPP_NULLPTR_SUPPORTED 0
#endif // !defined(VA_CPP_NULLPTR_SUPPORTED)

#ifdef __cplusplus
}
#endif

#endif
