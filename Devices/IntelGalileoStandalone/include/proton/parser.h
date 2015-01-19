#ifndef PROTON_PARSER_H
#define PROTON_PARSER_H 1

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
#include <proton/codec.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct pn_parser_t pn_parser_t;

PN_EXTERN pn_parser_t *pn_parser(void);
PN_EXTERN int pn_parser_parse(pn_parser_t *parser, const char *str, pn_data_t *data);
PN_EXTERN int pn_parser_errno(pn_parser_t *parser);
PN_EXTERN const char *pn_parser_error(pn_parser_t *parser);
PN_EXTERN void pn_parser_free(pn_parser_t *parser);

#ifdef __cplusplus
}
#endif

#endif /* parser.h */
