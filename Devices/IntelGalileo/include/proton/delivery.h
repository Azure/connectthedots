#ifndef PROTON_DELIVERY_H
#define PROTON_DELIVERY_H 1

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
#include <proton/disposition.h>
#include <proton/type_compat.h>
#include <stddef.h>
#include <sys/types.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * @file
 *
 * Delivery API for the proton Engine.
 *
 * @defgroup delivery Delivery
 * @ingroup engine
 * @{
 */

/**
 * An AMQP delivery tag.
 */
typedef struct pn_delivery_tag_t {
  size_t size;
  const char *bytes;
} pn_delivery_tag_t;

#ifndef SWIG  // older versions of SWIG choke on this:
/**
 * Construct a delivery tag.
 *
 * @param[in] bytes a pointer to the beginning of the tag
 * @param[in] size the size of the tag
 * @return the delivery tag
 */
static inline pn_delivery_tag_t pn_dtag(const char *bytes, size_t size) {
  pn_delivery_tag_t dtag = {size, bytes};
  return dtag;
}
#endif

/**
 * Create a delivery on a link.
 *
 * Every delivery object within a link must be supplied with a unique
 * tag. Links maintain a sequence of delivery object in the order that
 * they are created.
 *
 * @param[in] link a link object
 * @param[in] tag the delivery tag
 * @return a newly created delivery, or NULL if there was an error
 */
PN_EXTERN pn_delivery_t *pn_delivery(pn_link_t *link, pn_delivery_tag_t tag);

/**
 * Get the application context that is associated with a delivery object.
 *
 * The application context for a delivery may be set using
 * ::pn_delivery_set_context.
 *
 * @param[in] delivery the delivery whose context is to be returned.
 * @return the application context for the delivery object
 */
PN_EXTERN void *pn_delivery_get_context(pn_delivery_t *delivery);

/**
 * Set a new application context for a delivery object.
 *
 * The application context for a delivery object may be retrieved using
 * ::pn_delivery_get_context.
 *
 * @param[in] delivery the delivery object
 * @param[in] context the application context
 */
PN_EXTERN void pn_delivery_set_context(pn_delivery_t *delivery, void *context);

/**
 * Get the tag for a delivery object.
 *
 * @param[in] delivery a delivery object
 * @return the delivery tag
 */
PN_EXTERN pn_delivery_tag_t pn_delivery_tag(pn_delivery_t *delivery);

/**
 * Get the parent link for a delivery object.
 *
 * @param[in] delivery a delivery object
 * @return the parent link
 */
PN_EXTERN pn_link_t *pn_delivery_link(pn_delivery_t *delivery);

/**
 * Get the local disposition for a delivery.
 *
 * The pointer returned by this object is valid until the delivery is
 * settled.
 *
 * @param[in] delivery a delivery object
 * @return a pointer to the local disposition
 */
PN_EXTERN pn_disposition_t *pn_delivery_local(pn_delivery_t *delivery);

/**
 * Get the local disposition state for a delivery.
 *
 * @param[in] delivery a delivery object
 * @return the local disposition state
 */
PN_EXTERN uint64_t pn_delivery_local_state(pn_delivery_t *delivery);

/**
 * Get the remote disposition for a delivery.
 *
 * The pointer returned by this object is valid until the delivery is
 * settled.
 *
 * @param[in] delivery a delivery object
 * @return a pointer to the remote disposition
 */
PN_EXTERN pn_disposition_t *pn_delivery_remote(pn_delivery_t *delivery);

/**
 * Get the remote disposition state for a delivery.
 *
 * @param[in] delivery a delivery object
 * @return the remote disposition state
 */
PN_EXTERN uint64_t pn_delivery_remote_state(pn_delivery_t *delivery);

/**
 * Check if a delivery is remotely settled.
 *
 * @param[in] delivery a delivery object
 * @return true if the delivery is settled at the remote endpoint, false otherwise
 */
PN_EXTERN bool pn_delivery_settled(pn_delivery_t *delivery);

/**
 * Get the amount of pending message data for a delivery.
 *
 * @param[in] delivery a delivery object
 * @return the amount of pending message data in bytes
 */
PN_EXTERN size_t pn_delivery_pending(pn_delivery_t *delivery);

/**
 * Check if a delivery only has partial message data.
 *
 * @param[in] delivery a delivery object
 * @return true if the delivery only contains part of a message, false otherwise
 */
PN_EXTERN bool pn_delivery_partial(pn_delivery_t *delivery);

/**
 * Check if a delivery is writable.
 *
 * A delivery is considered writable if it is the current delivery on
 * an outgoing link, and the link has positive credit.
 *
 * @param[in] delivery a delivery object
 * @return true if the delivery is writable, false otherwise
 */
PN_EXTERN bool pn_delivery_writable(pn_delivery_t *delivery);

/**
 * Check if a delivery is readable.
 *
 * A delivery is considered readable if it is the current delivery on
 * an incoming link.
 *
 * @param[in] delivery a delivery object
 * @return true if the delivery is readable, false otherwise
 */
PN_EXTERN bool pn_delivery_readable(pn_delivery_t *delivery);

/**
 * Check if a delivery is updated.
 *
 * A delivery is considered updated whenever the peer communicates a
 * new disposition for the delivery. Once a delivery becomes updated,
 * it will remain so until ::pn_delivery_clear is called.
 *
 * @param[in] delivery a delivery object
 * @return true if the delivery is updated, false otherwise
 */
PN_EXTERN bool pn_delivery_updated(pn_delivery_t *delivery);

/**
 * Update the disposition of a delivery.
 *
 * When update is invoked the updated disposition of the delivery will
 * be communicated to the peer.
 *
 * @param[in] delivery a delivery object
 * @param[in] state the updated delivery state
 */
PN_EXTERN void pn_delivery_update(pn_delivery_t *delivery, uint64_t state);

/**
 * Clear the updated flag for a delivery.
 *
 * See ::pn_delivery_updated.
 *
 * @param[in] delivery a delivery object
 */
PN_EXTERN void pn_delivery_clear(pn_delivery_t *delivery);

//int pn_delivery_format(pn_delivery_t *delivery);

/**
 * Settle a delivery.
 *
 * A settled delivery can never be used again.
 *
 * @param[in] delivery a delivery object
 */
PN_EXTERN void pn_delivery_settle(pn_delivery_t *delivery);

/**
 * Utility function for printing details of a delivery.
 *
 * @param[in] delivery a delivery object
 */
PN_EXTERN void pn_delivery_dump(pn_delivery_t *delivery);

/**
 * Check if a delivery is buffered.
 *
 * A delivery that is buffered has not yet been written to the wire.
 *
 * Note that returning false does not imply that a delivery was
 * definitely written to the wire. If false is returned, it is not
 * known whether the delivery was actually written to the wire or not.
 *
 * @param[in] delivery a delivery object
 * @return true if the delivery is buffered
 */
PN_EXTERN bool pn_delivery_buffered(pn_delivery_t *delivery);

/**
 * Extracts the first delivery on the connection that has pending
 * operations.
 *
 * Retrieves the first delivery on the Connection that has pending
 * operations. A readable delivery indicates message data is waiting
 * to be read. A writable delivery indicates that message data may be
 * sent. An updated delivery indicates that the delivery's disposition
 * has changed. A delivery will never be both readable and writible,
 * but it may be both readable and updated or both writiable and
 * updated.
 *
 * @param[in] connection the connection
 * @return the first delivery object that needs to be serviced, else
 * NULL if none
 */
PN_EXTERN pn_delivery_t *pn_work_head(pn_connection_t *connection);

/**
 * Get the next delivery on the connection that needs has pending
 * operations.
 *
 * @param[in] delivery the previous delivery retrieved from
 *                     either pn_work_head or pn_work_next
 * @return the next delivery that has pending operations, else
 * NULL if none
 */
PN_EXTERN pn_delivery_t *pn_work_next(pn_delivery_t *delivery);

/** @}
 */

#ifdef __cplusplus
}
#endif

#endif /* delivery.h */
