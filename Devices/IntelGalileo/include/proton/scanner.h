#ifndef PROTON_SCANNER_H
#define PROTON_SCANNER_H 1

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
#include <sys/types.h>
#include <stdarg.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
  PN_TOK_LBRACE,
  PN_TOK_RBRACE,
  PN_TOK_LBRACKET,
  PN_TOK_RBRACKET,
  PN_TOK_EQUAL,
  PN_TOK_COMMA,
  PN_TOK_POS,
  PN_TOK_NEG,
  PN_TOK_DOT,
  PN_TOK_AT,
  PN_TOK_DOLLAR,
  PN_TOK_BINARY,
  PN_TOK_STRING,
  PN_TOK_SYMBOL,
  PN_TOK_ID,
  PN_TOK_FLOAT,
  PN_TOK_INT,
  PN_TOK_TRUE,
  PN_TOK_FALSE,
  PN_TOK_NULL,
  PN_TOK_EOS,
  PN_TOK_ERR
} pn_token_type_t;

typedef struct pn_scanner_t pn_scanner_t;

typedef struct {
  pn_token_type_t type;
  const char *start;
  size_t size;
} pn_token_t;

PN_EXTERN pn_scanner_t *pn_scanner(void);
PN_EXTERN void pn_scanner_free(pn_scanner_t *scanner);
PN_EXTERN pn_token_t pn_scanner_token(pn_scanner_t *scanner);
PN_EXTERN int pn_scanner_err(pn_scanner_t *scanner, int code, const char *fmt, ...);
PN_EXTERN int pn_scanner_verr(pn_scanner_t *scanner, int code, const char *fmt, va_list ap);
PN_EXTERN void pn_scanner_line_info(pn_scanner_t *scanner, int *line, int *col);
PN_EXTERN int pn_scanner_errno(pn_scanner_t *scanner);
PN_EXTERN const char *pn_scanner_error(pn_scanner_t *scanner);
PN_EXTERN int pn_scanner_start(pn_scanner_t *scanner, const char *input);
PN_EXTERN int pn_scanner_scan(pn_scanner_t *scanner);
PN_EXTERN int pn_scanner_shift(pn_scanner_t *scanner);

#ifdef __cplusplus
}
#endif

#endif /* scanner.h */
