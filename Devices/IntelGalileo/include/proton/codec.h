#ifndef PROTON_CODEC_H
#define PROTON_CODEC_H 1

/*
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 *
 */

#include <proton/import_export.h>
#include <proton/object.h>
#include <proton/types.h>
#include <proton/error.h>
#ifndef __cplusplus
#include <stdbool.h>
#include <stdint.h>
#else
#include <proton/type_compat.h>
#endif
#include <stdarg.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
  PN_NULL = 1,
  PN_BOOL = 2,
  PN_UBYTE = 3,
  PN_BYTE = 4,
  PN_USHORT = 5,
  PN_SHORT = 6,
  PN_UINT = 7,
  PN_INT = 8,
  PN_CHAR = 9,
  PN_ULONG = 10,
  PN_LONG = 11,
  PN_TIMESTAMP = 12,
  PN_FLOAT = 13,
  PN_DOUBLE = 14,
  PN_DECIMAL32 = 15,
  PN_DECIMAL64 = 16,
  PN_DECIMAL128 = 17,
  PN_UUID = 18,
  PN_BINARY = 19,
  PN_STRING = 20,
  PN_SYMBOL = 21,
  PN_DESCRIBED = 22,
  PN_ARRAY = 23,
  PN_LIST = 24,
  PN_MAP = 25
} pn_type_t;

PN_EXTERN const char *pn_type_name(pn_type_t type);

typedef struct {
  pn_type_t type;
  union {
    bool as_bool;
    uint8_t as_ubyte;
    int8_t as_byte;
    uint16_t as_ushort;
    int16_t as_short;
    uint32_t as_uint;
    int32_t as_int;
    pn_char_t as_char;
    uint64_t as_ulong;
    int64_t as_long;
    pn_timestamp_t as_timestamp;
    float as_float;
    double as_double;
    pn_decimal32_t as_decimal32;
    pn_decimal64_t as_decimal64;
    pn_decimal128_t as_decimal128;
    pn_uuid_t as_uuid;
    pn_bytes_t as_bytes;
  } u;
} pn_atom_t;

// data

typedef struct pn_data_t pn_data_t;

PN_EXTERN pn_data_t *pn_data(size_t capacity);
PN_EXTERN void pn_data_free(pn_data_t *data);
PN_EXTERN int pn_data_errno(pn_data_t *data);
PN_EXTERN pn_error_t *pn_data_error(pn_data_t *data);
PN_EXTERN int pn_data_vfill(pn_data_t *data, const char *fmt, va_list ap);
PN_EXTERN int pn_data_fill(pn_data_t *data, const char *fmt, ...);
PN_EXTERN int pn_data_vscan(pn_data_t *data, const char *fmt, va_list ap);
PN_EXTERN int pn_data_scan(pn_data_t *data, const char *fmt, ...);

PN_EXTERN void pn_data_clear(pn_data_t *data);
PN_EXTERN size_t pn_data_size(pn_data_t *data);
PN_EXTERN void pn_data_rewind(pn_data_t *data);
PN_EXTERN bool pn_data_next(pn_data_t *data);
PN_EXTERN bool pn_data_prev(pn_data_t *data);
PN_EXTERN bool pn_data_enter(pn_data_t *data);
PN_EXTERN bool pn_data_exit(pn_data_t *data);
PN_EXTERN bool pn_data_lookup(pn_data_t *data, const char *name);

PN_EXTERN pn_type_t pn_data_type(pn_data_t *data);

PN_EXTERN int pn_data_print(pn_data_t *data);
PN_EXTERN int pn_data_format(pn_data_t *data, char *bytes, size_t *size);
PN_EXTERN ssize_t pn_data_encode(pn_data_t *data, char *bytes, size_t size);
PN_EXTERN ssize_t pn_data_decode(pn_data_t *data, const char *bytes, size_t size);

PN_EXTERN int pn_data_put_list(pn_data_t *data);
PN_EXTERN int pn_data_put_map(pn_data_t *data);
PN_EXTERN int pn_data_put_array(pn_data_t *data, bool described, pn_type_t type);
PN_EXTERN int pn_data_put_described(pn_data_t *data);
PN_EXTERN int pn_data_put_null(pn_data_t *data);
PN_EXTERN int pn_data_put_bool(pn_data_t *data, bool b);
PN_EXTERN int pn_data_put_ubyte(pn_data_t *data, uint8_t ub);
PN_EXTERN int pn_data_put_byte(pn_data_t *data, int8_t b);
PN_EXTERN int pn_data_put_ushort(pn_data_t *data, uint16_t us);
PN_EXTERN int pn_data_put_short(pn_data_t *data, int16_t s);
PN_EXTERN int pn_data_put_uint(pn_data_t *data, uint32_t ui);
PN_EXTERN int pn_data_put_int(pn_data_t *data, int32_t i);
PN_EXTERN int pn_data_put_char(pn_data_t *data, pn_char_t c);
PN_EXTERN int pn_data_put_ulong(pn_data_t *data, uint64_t ul);
PN_EXTERN int pn_data_put_long(pn_data_t *data, int64_t l);
PN_EXTERN int pn_data_put_timestamp(pn_data_t *data, pn_timestamp_t t);
PN_EXTERN int pn_data_put_float(pn_data_t *data, float f);
PN_EXTERN int pn_data_put_double(pn_data_t *data, double d);
PN_EXTERN int pn_data_put_decimal32(pn_data_t *data, pn_decimal32_t d);
PN_EXTERN int pn_data_put_decimal64(pn_data_t *data, pn_decimal64_t d);
PN_EXTERN int pn_data_put_decimal128(pn_data_t *data, pn_decimal128_t d);
PN_EXTERN int pn_data_put_uuid(pn_data_t *data, pn_uuid_t u);
PN_EXTERN int pn_data_put_binary(pn_data_t *data, pn_bytes_t bytes);
PN_EXTERN int pn_data_put_string(pn_data_t *data, pn_bytes_t string);
PN_EXTERN int pn_data_put_symbol(pn_data_t *data, pn_bytes_t symbol);
PN_EXTERN int pn_data_put_atom(pn_data_t *data, pn_atom_t atom);

PN_EXTERN size_t pn_data_get_list(pn_data_t *data);
PN_EXTERN size_t pn_data_get_map(pn_data_t *data);
PN_EXTERN size_t pn_data_get_array(pn_data_t *data);
PN_EXTERN bool pn_data_is_array_described(pn_data_t *data);
PN_EXTERN pn_type_t pn_data_get_array_type(pn_data_t *data);
PN_EXTERN bool pn_data_is_described(pn_data_t *data);
PN_EXTERN bool pn_data_is_null(pn_data_t *data);
PN_EXTERN bool pn_data_get_bool(pn_data_t *data);
PN_EXTERN uint8_t pn_data_get_ubyte(pn_data_t *data);
PN_EXTERN int8_t pn_data_get_byte(pn_data_t *data);
PN_EXTERN uint16_t pn_data_get_ushort(pn_data_t *data);
PN_EXTERN int16_t pn_data_get_short(pn_data_t *data);
PN_EXTERN uint32_t pn_data_get_uint(pn_data_t *data);
PN_EXTERN int32_t pn_data_get_int(pn_data_t *data);
PN_EXTERN pn_char_t pn_data_get_char(pn_data_t *data);
PN_EXTERN uint64_t pn_data_get_ulong(pn_data_t *data);
PN_EXTERN int64_t pn_data_get_long(pn_data_t *data);
PN_EXTERN pn_timestamp_t pn_data_get_timestamp(pn_data_t *data);
PN_EXTERN float pn_data_get_float(pn_data_t *data);
PN_EXTERN double pn_data_get_double(pn_data_t *data);
PN_EXTERN pn_decimal32_t pn_data_get_decimal32(pn_data_t *data);
PN_EXTERN pn_decimal64_t pn_data_get_decimal64(pn_data_t *data);
PN_EXTERN pn_decimal128_t pn_data_get_decimal128(pn_data_t *data);
PN_EXTERN pn_uuid_t pn_data_get_uuid(pn_data_t *data);
PN_EXTERN pn_bytes_t pn_data_get_binary(pn_data_t *data);
PN_EXTERN pn_bytes_t pn_data_get_string(pn_data_t *data);
PN_EXTERN pn_bytes_t pn_data_get_symbol(pn_data_t *data);
PN_EXTERN pn_bytes_t pn_data_get_bytes(pn_data_t *data);
PN_EXTERN pn_atom_t pn_data_get_atom(pn_data_t *data);

PN_EXTERN int pn_data_copy(pn_data_t *data, pn_data_t *src);
PN_EXTERN int pn_data_append(pn_data_t *data, pn_data_t *src);
PN_EXTERN int pn_data_appendn(pn_data_t *data, pn_data_t *src, int limit);
PN_EXTERN void pn_data_narrow(pn_data_t *data);
PN_EXTERN void pn_data_widen(pn_data_t *data);
PN_EXTERN pn_handle_t pn_data_point(pn_data_t *data);
PN_EXTERN bool pn_data_restore(pn_data_t *data, pn_handle_t point);


PN_EXTERN void pn_data_dump(pn_data_t *data);

#ifdef __cplusplus
}
#endif

#endif /* codec.h */
