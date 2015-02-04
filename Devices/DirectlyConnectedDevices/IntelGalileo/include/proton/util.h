#ifndef PROTON_UTIL_H
#define PROTON_UTIL_H 1

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
#include <stdarg.h>

#ifdef __cplusplus
extern "C" {
#endif

PN_EXTERN void parse_url(char *url, char **scheme, char **user, char **pass, char **host, char **port, char **path);
PN_EXTERN void pn_fatal(const char *fmt, ...);
PN_EXTERN void pn_vfatal(const char *fmt, va_list ap);

#ifdef __cplusplus
}
#endif

#endif /* util.h */
