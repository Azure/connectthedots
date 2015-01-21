#ifndef PROTON_CONDITION_H
#define PROTON_CONDITION_H 1

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
#include <proton/type_compat.h>
#include <stddef.h>
#include <sys/types.h>

#ifdef __cplusplus
extern "C" {
#endif

/** @file
 *
 * The Condition API for the proton Engine.
 *
 * @defgroup condition Condition
 * @ingroup connection
 * @{
 */

/**
 * An AMQP Condition object. Conditions hold exceptional information
 * pertaining to the closing of an AMQP endpoint such as a Connection,
 * Session, or Link. Conditions also hold similar information
 * pertaining to deliveries that have reached terminal states.
 * Connections, Sessions, Links, and Deliveries may all have local and
 * remote conditions associated with them.
 *
 * The local condition may be modified by the local endpoint to signal
 * a particular condition to the remote peer. The remote condition may
 * be examined by the local endpoint to detect whatever condition the
 * remote peer may be signaling. Although often conditions are used to
 * indicate errors, not all conditions are errors per/se, e.g.
 * conditions may be used to redirect a connection from one host to
 * another.
 *
 * Every condition has a short symbolic name, a longer description,
 * and an additional info map associated with it. The name identifies
 * the formally defined condition, and the map contains additional
 * information relevant to the identified condition.
 */
typedef struct pn_condition_t pn_condition_t;

/**
 * Returns true if the condition object is holding some information,
 * i.e. if the name is set to some non NULL value. Returns false
 * otherwise.
 *
 * @param[in] condition the condition object to test
 * @return true iff some condition information is set
 */
PN_EXTERN bool pn_condition_is_set(pn_condition_t *condition);

/**
 * Clears the condition object of any exceptional information. After
 * calling ::pn_condition_clear(), ::pn_condition_is_set() is
 * guaranteed to return false and ::pn_condition_get_name() as well as
 * ::pn_condition_get_description() will return NULL. The ::pn_data_t
 * returned by ::pn_condition_info() will still be valid, but will
 * have been cleared as well (See ::pn_data_clear()).
 *
 * @param[in] condition the condition object to clear
 */
PN_EXTERN void pn_condition_clear(pn_condition_t *condition);

/**
 * Returns the name associated with the exceptional condition, or NULL
 * if there is no conditional information set.
 *
 * @param[in] condition the condition object
 * @return a pointer to the name, or NULL
 */
PN_EXTERN const char *pn_condition_get_name(pn_condition_t *condition);

/**
 * Sets the name associated with the exceptional condition.
 *
 * @param[in] condition the condition object
 * @param[in] name the desired name
 * @return an error code or 0 on success
 */
PN_EXTERN int pn_condition_set_name(pn_condition_t *condition, const char *name);

/**
 * Gets the description associated with the exceptional condition.
 *
 * @param[in] condition the condition object
 * @return a pointer to the description, or NULL
 */
PN_EXTERN const char *pn_condition_get_description(pn_condition_t *condition);

/**
 * Sets the description associated with the exceptional condition.
 *
 * @param[in] condition the condition object
 * @param[in] description the desired description
 * @return an error code or 0 on success
 */
PN_EXTERN int pn_condition_set_description(pn_condition_t *condition, const char *description);

/**
 * Returns a data object that holds the additional information
 * associated with the condition. The data object may be used both to
 * access and to modify the additional information associated with the
 * condition.
 *
 * @param[in] condition the condition object
 * @return a data object holding the additional information for the condition
 */
PN_EXTERN pn_data_t *pn_condition_info(pn_condition_t *condition);

/**
 * Returns true if the condition is a redirect.
 *
 * @param[in] condition the condition object
 * @return true if the condition is a redirect, false otherwise
 */
PN_EXTERN bool pn_condition_is_redirect(pn_condition_t *condition);

/**
 * Retrieves the redirect host from the additional information
 * associated with the condition. If the condition is not a redirect,
 * this will return NULL.
 *
 * @param[in] condition the condition object
 * @return the redirect host or NULL
 */
PN_EXTERN const char *pn_condition_redirect_host(pn_condition_t *condition);

/**
 * Retrieves the redirect port from the additional information
 * associated with the condition. If the condition is not a redirect,
 * this will return an error code.
 *
 * @param[in] condition the condition object
 * @return the redirect port or an error code
 */
PN_EXTERN int pn_condition_redirect_port(pn_condition_t *condition);

/** @}
 */

#ifdef __cplusplus
}
#endif

#endif /* condition.h */
