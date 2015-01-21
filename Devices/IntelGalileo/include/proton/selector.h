#ifndef PROTON_SELECTOR_H
#define PROTON_SELECTOR_H 1

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
#include <proton/selectable.h>
#include <proton/type_compat.h>

#ifdef __cplusplus
extern "C" {
#endif

#define PN_READABLE (1)
#define PN_WRITABLE (2)
#define PN_EXPIRED (4)

pn_selector_t *pni_selector(void);
PN_EXTERN void pn_selector_free(pn_selector_t *selector);
PN_EXTERN void pn_selector_add(pn_selector_t *selector, pn_selectable_t *selectable);
PN_EXTERN void pn_selector_update(pn_selector_t *selector, pn_selectable_t *selectable);
PN_EXTERN void pn_selector_remove(pn_selector_t *selector, pn_selectable_t *selectable);
PN_EXTERN int pn_selector_select(pn_selector_t *select, int timeout);
PN_EXTERN pn_selectable_t *pn_selector_next(pn_selector_t *select, int *events);

#ifdef __cplusplus
}
#endif

#endif /* selector.h */
