#ifndef PROTON_TERMINUS_H
#define PROTON_TERMINUS_H 1

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
#include <proton/type_compat.h>
#include <stddef.h>
#include <sys/types.h>

#ifdef __cplusplus
extern "C" {
#endif

/** @file
 *
 * Terminus API for the proton Engine.
 *
 * @defgroup terminus Terminus
 * @ingroup link
 * @{
 */

/**
 * Encapsulates the endpoint state associated with an AMQP Terminus.
 *
 * An AMQP Terminus acts as either a source or target for messages,
 * but never both. Every AMQP link is associated with both a source
 * terminus and a target terminus that is negotiated during link
 * establishment. A terminus consists of an AMQP address, along with a
 * number of other properties defining the quality of service and
 * behaviour of the link.
 */
typedef struct pn_terminus_t pn_terminus_t;

/**
 * Type of an AMQP terminus.
 */
typedef enum {
  PN_UNSPECIFIED = 0, /**< indicates a nonexistent terminus, may used
                         as a source or target */
  PN_SOURCE = 1, /**< indicates a source of messages */
  PN_TARGET = 2, /**< indicates a target for messages */
  PN_COORDINATOR = 3 /**< a special target identifying a transaction
                        coordinator */
} pn_terminus_type_t;

/**
 * Durability mode of an AMQP terminus.
 *
 * An AMQP terminus may provide durable storage for its state, thereby
 * permitting link recovery in the event of endpoint failures. This
 * durability may be applied to the configuration of the terminus
 * only, or to all delivery state as well.
 */
typedef enum {
  PN_NONDURABLE = 0, /**< indicates a non durable terminus */
  PN_CONFIGURATION = 1, /**< indicates a terminus with durably held
                           configuration, but not delivery state */
  PN_DELIVERIES = 2 /**< indicates a terminus with both durably held
                       configuration and durably held delivery
                       state. */
} pn_durability_t;

/**
 * Expiry policy of an AMQP terminus.
 *
 * An orphaned terminus can only exist for the timeout configured by
 * ::pn_terminus_set_timeout. The expiry policy determins when a
 * terminus is considered orphaned, i.e. when the expiry timer starts
 * counting down.
 */
typedef enum {
  PN_EXPIRE_WITH_LINK, /**< the terminus is orphaned when the parent link is closed */
  PN_EXPIRE_WITH_SESSION, /**< the terminus is orphaned when the parent session is closed */
  PN_EXPIRE_WITH_CONNECTION, /**< the terminus is orphaned when the parent connection is closed */
  PN_EXPIRE_NEVER /**< the terminus is never considered orphaned */
} pn_expiry_policy_t;

/**
 * Distribution mode of an AMQP terminus.
 *
 * The distribution mode of a source terminus defines the behaviour
 * when multiple receiving links provide addresses that resolve to the
 * same node.
 */
typedef enum {
  PN_DIST_MODE_UNSPECIFIED = 0, /**< the behaviour is defined by the node */
  PN_DIST_MODE_COPY = 1, /**< the receiver gets all messages */
  PN_DIST_MODE_MOVE = 2 /**< the receiver competes for messages */
} pn_distribution_mode_t;

/**
 * Get the type of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @return the terminus type
 */
PN_EXTERN pn_terminus_type_t pn_terminus_get_type(pn_terminus_t *terminus);

/**
 * Set the type of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @param[in] type the terminus type
 * @return 0 on success or an error code on failure
 */
PN_EXTERN int pn_terminus_set_type(pn_terminus_t *terminus, pn_terminus_type_t type);

/**
 * Get the address of a terminus object.
 *
 * The pointer returned by this operation is valid until
 * ::pn_terminus_set_address is called or until the terminus is freed
 * due to its parent link being freed.
 *
 * @param[in] terminus a terminus object
 * @return a pointer to the address
 */
PN_EXTERN const char *pn_terminus_get_address(pn_terminus_t *terminus);

/**
 * Set the address of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @param[in] address an AMQP address string
 * @return 0 on success or an error code on failure
 */
PN_EXTERN int pn_terminus_set_address(pn_terminus_t *terminus, const char *address);

/**
 * Get the distribution mode of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @return the distribution mode of the terminus
 */
PN_EXTERN pn_distribution_mode_t pn_terminus_get_distribution_mode(const pn_terminus_t *terminus);

/**
 * Set the distribution mode of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @param[in] mode the distribution mode for the terminus
 * @return 0 on success or an error code on failure
 */
PN_EXTERN int pn_terminus_set_distribution_mode(pn_terminus_t *terminus, pn_distribution_mode_t mode);

/**
 * Get the durability mode of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @return the terminus durability mode
 */
PN_EXTERN pn_durability_t pn_terminus_get_durability(pn_terminus_t *terminus);

/**
 * Set the durability mode of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @param[in] durability the terminus durability mode
 * @return 0 on success or an error code on failure
 */
PN_EXTERN int pn_terminus_set_durability(pn_terminus_t *terminus,
                                         pn_durability_t durability);

/**
 * Get the expiry policy of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @return the expiry policy of the terminus
 */
PN_EXTERN pn_expiry_policy_t pn_terminus_get_expiry_policy(pn_terminus_t *terminus);

/**
 * Set the expiry policy of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @param[in] policy the expiry policy for the terminus
 * @return 0 on success or an error code on failure
 */
PN_EXTERN int pn_terminus_set_expiry_policy(pn_terminus_t *terminus, pn_expiry_policy_t policy);

/**
 * Get the timeout of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @return the timeout of the terminus
 */
PN_EXTERN pn_seconds_t pn_terminus_get_timeout(pn_terminus_t *terminus);

/**
 * Set the timeout of a terminus object.
 *
 * @param[in] terminus a terminus object
 * @param[in] timeout the timeout for the terminus
 * @return 0 on success or an error code on failure
 */
PN_EXTERN int pn_terminus_set_timeout(pn_terminus_t *terminus, pn_seconds_t timeout);

/**
 * Get the dynamic flag for a terminus object.
 *
 * @param[in] terminus a terminus object
 * @return true if the dynamic flag is set for the terminus, false otherwise
 */
PN_EXTERN bool pn_terminus_is_dynamic(pn_terminus_t *terminus);

/**
 * Set the dynamic flag for a terminus object.
 *
 * @param[in] terminus a terminus object
 * @param[in] dynamic the dynamic flag for the terminus
 * @return 0 on success or an error code on failure
 */
PN_EXTERN int pn_terminus_set_dynamic(pn_terminus_t *terminus, bool dynamic);

/**
 * Access/modify the AMQP properties data for a terminus object.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the terminus object is freed due to its parent link
 * being freed. Any data contained by the ::pn_data_t object will be
 * sent as the AMQP properties for the parent terminus object. Note
 * that this MUST take the form of a symbol keyed map to be valid.
 *
 * @param[in] terminus a terminus object
 * @return a pointer to a pn_data_t representing the terminus properties
 */
PN_EXTERN pn_data_t *pn_terminus_properties(pn_terminus_t *terminus);

/**
 * Access/modify the AMQP capabilities data for a terminus object.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the terminus object is freed due to its parent link
 * being freed. Any data contained by the ::pn_data_t object will be
 * sent as the AMQP capabilities for the parent terminus object. Note
 * that this MUST take the form of an array of symbols to be valid.
 *
 * @param[in] terminus a terminus object
 * @return a pointer to a pn_data_t representing the terminus capabilities
 */
PN_EXTERN pn_data_t *pn_terminus_capabilities(pn_terminus_t *terminus);

/**
 * Access/modify the AMQP outcomes for a terminus object.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the terminus object is freed due to its parent link
 * being freed. Any data contained by the ::pn_data_t object will be
 * sent as the AMQP outcomes for the parent terminus object. Note
 * that this MUST take the form of an array of symbols to be valid.
 *
 * @param[in] terminus a terminus object
 * @return a pointer to a pn_data_t representing the terminus outcomes
 */
PN_EXTERN pn_data_t *pn_terminus_outcomes(pn_terminus_t *terminus);

/**
 * Access/modify the AMQP filter set for a terminus object.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the terminus object is freed due to its parent link
 * being freed. Any data contained by the ::pn_data_t object will be
 * sent as the AMQP filter set for the parent terminus object. Note
 * that this MUST take the form of a symbol keyed map to be valid.
 *
 * @param[in] terminus a source terminus object
 * @return a pointer to a pn_data_t representing the terminus filter set
 */
PN_EXTERN pn_data_t *pn_terminus_filter(pn_terminus_t *terminus);

/**
 * Copy a terminus object.
 *
 * @param[in] terminus the terminus object to be copied into
 * @param[in] src the terminus to be copied from
 * @return 0 on success or an error code on failure
 */
PN_EXTERN int pn_terminus_copy(pn_terminus_t *terminus, pn_terminus_t *src);

/** @}
 */

#ifdef __cplusplus
}
#endif

#endif /* terminus.h */
