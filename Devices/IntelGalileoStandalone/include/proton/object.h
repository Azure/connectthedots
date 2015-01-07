#ifndef PROTON_OBJECT_H
#define PROTON_OBJECT_H 1

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

#include <proton/types.h>
#include <stdarg.h>
#ifndef __cplusplus
#include <stdbool.h>
#include <stdint.h>
#else
#include <proton/type_compat.h>
#endif
#include <stddef.h>
#include <proton/import_export.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef uintptr_t pn_handle_t;
typedef intptr_t pn_shandle_t;

typedef struct pn_list_t pn_list_t;
typedef struct pn_map_t pn_map_t;
typedef struct pn_hash_t pn_hash_t;
typedef struct pn_string_t pn_string_t;

typedef struct {
  void (*initialize)(void *);
  void (*finalize)(void *);
  uintptr_t (*hashcode)(void *);
  intptr_t (*compare)(void *, void *);
  int (*inspect)(void *, pn_string_t *);
} pn_class_t;

#define PN_CLASS(PREFIX) {                      \
    PREFIX ## _initialize,                      \
    PREFIX ## _finalize,                        \
    PREFIX ## _hashcode,                        \
    PREFIX ## _compare,                         \
    PREFIX ## _inspect                          \
}

PN_EXTERN void *pn_new(size_t size, pn_class_t *clazz);
PN_EXTERN void pn_initialize(void *object, pn_class_t *clazz);
PN_EXTERN void *pn_incref(void *object);
PN_EXTERN void pn_decref(void *object);
PN_EXTERN int pn_refcount(void *object);
PN_EXTERN void pn_finalize(void *object);
PN_EXTERN void pn_free(void *object);
PN_EXTERN pn_class_t *pn_class(void *object);
PN_EXTERN uintptr_t pn_hashcode(void *object);
PN_EXTERN intptr_t pn_compare(void *a, void *b);
PN_EXTERN bool pn_equals(void *a, void *b);
PN_EXTERN int pn_inspect(void *object, pn_string_t *dst);

#define PN_REFCOUNT (0x1)

PN_EXTERN pn_list_t *pn_list(size_t capacity, int options);
PN_EXTERN size_t pn_list_size(pn_list_t *list);
PN_EXTERN void *pn_list_get(pn_list_t *list, int index);
PN_EXTERN void pn_list_set(pn_list_t *list, int index, void *value);
PN_EXTERN int pn_list_add(pn_list_t *list, void *value);
PN_EXTERN ssize_t pn_list_index(pn_list_t *list, void *value);
PN_EXTERN bool pn_list_remove(pn_list_t *list, void *value);
PN_EXTERN void pn_list_del(pn_list_t *list, int index, int n);

#define PN_REFCOUNT_KEY (0x2)
#define PN_REFCOUNT_VALUE (0x4)

PN_EXTERN pn_map_t *pn_map(size_t capacity, float load_factor, int options);
PN_EXTERN size_t pn_map_size(pn_map_t *map);
PN_EXTERN int pn_map_put(pn_map_t *map, void *key, void *value);
PN_EXTERN void *pn_map_get(pn_map_t *map, void *key);
PN_EXTERN void pn_map_del(pn_map_t *map, void *key);
PN_EXTERN pn_handle_t pn_map_head(pn_map_t *map);
PN_EXTERN pn_handle_t pn_map_next(pn_map_t *map, pn_handle_t entry);
PN_EXTERN void *pn_map_key(pn_map_t *map, pn_handle_t entry);
PN_EXTERN void *pn_map_value(pn_map_t *map, pn_handle_t entry);

PN_EXTERN pn_hash_t *pn_hash(size_t capacity, float load_factor, int options);
PN_EXTERN size_t pn_hash_size(pn_hash_t *hash);
PN_EXTERN int pn_hash_put(pn_hash_t *hash, uintptr_t key, void *value);
PN_EXTERN void *pn_hash_get(pn_hash_t *hash, uintptr_t key);
PN_EXTERN void pn_hash_del(pn_hash_t *hash, uintptr_t key);
PN_EXTERN pn_handle_t pn_hash_head(pn_hash_t *hash);
PN_EXTERN pn_handle_t pn_hash_next(pn_hash_t *hash, pn_handle_t entry);
PN_EXTERN uintptr_t pn_hash_key(pn_hash_t *hash, pn_handle_t entry);
PN_EXTERN void *pn_hash_value(pn_hash_t *hash, pn_handle_t entry);

PN_EXTERN pn_string_t *pn_string(const char *bytes);
PN_EXTERN pn_string_t *pn_stringn(const char *bytes, size_t n);
PN_EXTERN const char *pn_string_get(pn_string_t *string);
PN_EXTERN size_t pn_string_size(pn_string_t *string);
PN_EXTERN int pn_string_set(pn_string_t *string, const char *bytes);
PN_EXTERN int pn_string_setn(pn_string_t *string, const char *bytes, size_t n);
PN_EXTERN ssize_t pn_string_put(pn_string_t *string, char *dst);
PN_EXTERN void pn_string_clear(pn_string_t *string);
PN_EXTERN int pn_string_format(pn_string_t *string, const char *format, ...)
#ifdef __GNUC__
  __attribute__ ((format (printf, 2, 3)))
#endif
    ;
PN_EXTERN int pn_string_vformat(pn_string_t *string, const char *format, va_list ap);
PN_EXTERN int pn_string_addf(pn_string_t *string, const char *format, ...)
#ifdef __GNUC__
  __attribute__ ((format (printf, 2, 3)))
#endif
    ;
PN_EXTERN int pn_string_vaddf(pn_string_t *string, const char *format, va_list ap);
PN_EXTERN int pn_string_grow(pn_string_t *string, size_t capacity);
PN_EXTERN char *pn_string_buffer(pn_string_t *string);
PN_EXTERN size_t pn_string_capacity(pn_string_t *string);
PN_EXTERN int pn_string_resize(pn_string_t *string, size_t size);
PN_EXTERN int pn_string_copy(pn_string_t *string, pn_string_t *src);


#ifdef __cplusplus
}
#endif

#endif /* object.h */
