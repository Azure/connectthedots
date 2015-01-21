#ifndef PROTON_DISPOSITION_H
#define PROTON_DISPOSITION_H 1

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

/**
 * @file
 *
 * Disposition API for the proton Engine.
 *
 * @defgroup disposition Disposition
 * @ingroup delivery
 * @{
 */

/**
 * Dispositions record the current state and/or final outcome of a
 * transfer. Every delivery contains both a local and remote
 * disposition. The local disposition holds the local state of the
 * delivery, and the remote disposition holds the last known remote
 * state of the delivery.
 */
typedef struct pn_disposition_t pn_disposition_t;

/**
 * The PN_RECEIVED delivery state is a non terminal state indicating
 * how much (if any) message data has been received for a delivery.
 */
#define PN_RECEIVED (0x0000000000000023)

/**
 * The PN_ACCEPTED delivery state is a terminal state indicating that
 * the delivery was successfully processed. Once in this state there
 * will be no further state changes prior to the delivery being
 * settled.
 */
#define PN_ACCEPTED (0x0000000000000024)

/**
 * The PN_REJECTED delivery state is a terminal state indicating that
 * the delivery could not be processed due to some error condition.
 * Once in this state there will be no further state changes prior to
 * the delivery being settled.
 */
#define PN_REJECTED (0x0000000000000025)

/**
 * The PN_RELEASED delivery state is a terminal state indicating that
 * the delivery is being returned to the sender. Once in this state
 * there will be no further state changes prior to the delivery being
 * settled.
 */
#define PN_RELEASED (0x0000000000000026)

/**
 * The PN_MODIFIED delivery state is a terminal state indicating that
 * the delivery is being returned to the sender and should be
 * annotated by the sender prior to further delivery attempts. Once in
 * this state there will be no further state changes prior to the
 * delivery being settled.
 */
#define PN_MODIFIED (0x0000000000000027)

/**
 * Get the type of a disposition.
 *
 * Defined values are:
 *
 *  - ::PN_RECEIVED
 *  - ::PN_ACCEPTED
 *  - ::PN_REJECTED
 *  - ::PN_RELEASED
 *  - ::PN_MODIFIED
 *
 * @param[in] disposition a disposition object
 * @return the type of the disposition
 */
PN_EXTERN uint64_t pn_disposition_type(pn_disposition_t *disposition);

/**
 * Access the condition object associated with a disposition.
 *
 * The ::pn_condition_t object retrieved by this operation may be
 * modified prior to updating a delivery. When a delivery is updated,
 * the condition described by the disposition is reported to the peer
 * if applicable to the current delivery state, e.g. states such as
 * ::PN_REJECTED.
 *
 * The pointer returned by this operation is valid until the parent
 * delivery is settled.
 *
 * @param[in] disposition a disposition object
 * @return a pointer to the disposition condition
 */
PN_EXTERN pn_condition_t *pn_disposition_condition(pn_disposition_t *disposition);

/**
 * Access the disposition as a raw pn_data_t.
 *
 * Dispositions are an extension point in the AMQP protocol. The
 * disposition interface provides setters/getters for those
 * dispositions that are predefined by the specification, however
 * access to the raw disposition data is provided so that other
 * dispositions can be used.
 *
 * The ::pn_data_t pointer returned by this operation is valid until
 * the parent delivery is settled.
 *
 * @param[in] disposition a disposition object
 * @return a pointer to the raw disposition data
 */
PN_EXTERN pn_data_t *pn_disposition_data(pn_disposition_t *disposition);

/**
 * Get the section number associated with a disposition.
 *
 * @param[in] disposition a disposition object
 * @return a section number
 */
PN_EXTERN uint32_t pn_disposition_get_section_number(pn_disposition_t *disposition);

/**
 * Set the section number associated with a disposition.
 *
 * @param[in] disposition a disposition object
 * @param[in] section_number a section number
 */
PN_EXTERN void pn_disposition_set_section_number(pn_disposition_t *disposition, uint32_t section_number);

/**
 * Get the section offset associated with a disposition.
 *
 * @param[in] disposition a disposition object
 * @return a section offset
 */
PN_EXTERN uint64_t pn_disposition_get_section_offset(pn_disposition_t *disposition);

/**
 * Set the section offset associated with a disposition.
 *
 * @param[in] disposition a disposition object
 * @param[in] section_offset a section offset
 */
PN_EXTERN void pn_disposition_set_section_offset(pn_disposition_t *disposition, uint64_t section_offset);

/**
 * Check if a disposition has the failed flag set.
 *
 * @param[in] disposition a disposition object
 * @return true if the disposition has the failed flag set, false otherwise
 */
PN_EXTERN bool pn_disposition_is_failed(pn_disposition_t *disposition);

/**
 * Set the failed flag on a disposition.
 *
 * @param[in] disposition a disposition object
 * @param[in] failed the value of the failed flag
 */
PN_EXTERN void pn_disposition_set_failed(pn_disposition_t *disposition, bool failed);

/**
 * Check if a disposition has the undeliverable flag set.
 *
 * @param[in] disposition a disposition object
 * @return true if the disposition has the undeliverable flag set, false otherwise
 */
PN_EXTERN bool pn_disposition_is_undeliverable(pn_disposition_t *disposition);

/**
 * Set the undeliverable flag on a disposition.
 *
 * @param[in] disposition a disposition object
 * @param[in] undeliverable the value of the undeliverable flag
 */
PN_EXTERN void pn_disposition_set_undeliverable(pn_disposition_t *disposition, bool undeliverable);

/**
 * Access the annotations associated with a disposition.
 *
 * The ::pn_data_t object retrieved by this operation may be modified
 * prior to updating a delivery. When a delivery is updated, the
 * annotations described by the ::pn_data_t are reported to the peer
 * if applicable to the current delivery state, e.g. states such as
 * ::PN_MODIFIED. The ::pn_data_t must be empty or contain a symbol
 * keyed map.
 *
 * The pointer returned by this operation is valid until the parent
 * delivery is settled.
 *
 * @param[in] disposition a disposition object
 * @return the annotations associated with the disposition
 */
PN_EXTERN pn_data_t *pn_disposition_annotations(pn_disposition_t *disposition);

/** @}
 */

#ifdef __cplusplus
}
#endif

#endif /* disposition.h */
