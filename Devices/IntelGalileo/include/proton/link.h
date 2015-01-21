#ifndef PROTON_LINK_H
#define PROTON_LINK_H 1

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
 * Link API for the proton Engine.
 *
 * @defgroup link Link
 * @ingroup engine
 * @{
 */

/**
 * Construct a new sender on a session.
 *
 * Each sending link between two AMQP containers must be uniquely
 * named. Note that this uniqueness cannot be enforced at the API
 * level, so some consideration should be taken in choosing link
 * names.
 *
 * @param[in] session the session object
 * @param[in] name the name of the link
 * @return a newly constructed sender link or NULL on error
 */
PN_EXTERN pn_link_t *pn_sender(pn_session_t *session, const char *name);

/**
 * Construct a new receiver on a session.
 *
 * Each receiving link between two AMQP containers must be uniquely
 * named. Note that this uniqueness cannot be enforced at the API
 * level, so some consideration should be taken in choosing link
 * names.
 *
 * @param[in] session the session object
 * @param[in] name the name of the link
 * @return a newly constructed receiver link or NULL on error
 */
PN_EXTERN pn_link_t *pn_receiver(pn_session_t *session, const char *name);

/**
 * Free a link object.
 *
 * When a link object is freed, all ::pn_delivery_t objects associated
 * with the session are also freed.
 *
 * @param[in] link a link object to free (or NULL)
 */
PN_EXTERN void pn_link_free(pn_link_t *link);

/**
 * Get the application context that is associated with a link object.
 *
 * The application context for a link may be set using
 * ::pn_link_set_context.
 *
 * @param[in] link the link whose context is to be returned.
 * @return the application context for the link object
 */
PN_EXTERN void *pn_link_get_context(pn_link_t *link);

/**
 * Set a new application context for a link object.
 *
 * The application context for a link object may be retrieved using
 * ::pn_link_get_context.
 *
 * @param[in] link the link object
 * @param[in] context the application context
 */
PN_EXTERN void pn_link_set_context(pn_link_t *link, void *context);

/**
 * Get the name of a link.
 *
 * @param[in] link a link object
 * @return the name of the link
 */
PN_EXTERN const char *pn_link_name(pn_link_t *link);

/**
 * Test if a link is a sender.
 *
 * @param[in] link a link object
 * @return true if and only if the link is a sender
 */
PN_EXTERN bool pn_link_is_sender(pn_link_t *link);

/**
 * Test if a link is a receiver.
 *
 * @param[in] link a link object
 * @return true if and only if the link is a receiver
 */
PN_EXTERN bool pn_link_is_receiver(pn_link_t *link);

/**
 * Get the endpoint state flags for a link.
 *
 * @param[in] link the link
 * @return the link's state flags
 */
PN_EXTERN pn_state_t pn_link_state(pn_link_t *link);

/**
 * Get additional error information associated with the link.
 *
 * Whenever a link operation fails (i.e. returns an error code),
 * additional error details can be obtained using this function. The
 * error object that is returned may also be used to clear the error
 * condition.
 *
 * The pointer returned by this operation is valid until the
 * link object is freed.
 *
 * @param[in] link the link object
 * @return the link's error object
 */
PN_EXTERN pn_error_t *pn_link_error(pn_link_t *link);

/**
 * Get the local condition associated with a link endpoint.
 *
 * The ::pn_condition_t object retrieved may be modified prior to
 * closing a link in order to indicate a particular condition
 * exists when the link closes. This is normally used to
 * communicate error conditions to the remote peer, however it may
 * also be used in non error cases. See ::pn_condition_t for more
 * details.
 *
 * The pointer returned by this operation is valid until the link
 * object is freed.
 *
 * @param[in] link the link object
 * @return the link's local condition object
 */
PN_EXTERN pn_condition_t *pn_link_condition(pn_link_t *link);

/**
 * Get the remote condition associated with a link endpoint.
 *
 * The ::pn_condition_t object retrieved may be examined in order to
 * determine whether the remote peer was indicating some sort of
 * exceptional condition when the remote link endpoint was
 * closed. The ::pn_condition_t object returned may not be modified.
 *
 * The pointer returned by this operation is valid until the
 * link object is freed.
 *
 * @param[in] link the link object
 * @return the link's remote condition object
 */
PN_EXTERN pn_condition_t *pn_link_remote_condition(pn_link_t *link);

/**
 * Get the parent session for a link object.
 *
 * This operation retrieves the parent ::pn_session_t object that
 * contains the given ::pn_link_t object.
 *
 * @param[in] link the link object
 * @return the parent session object
 */
PN_EXTERN pn_session_t *pn_link_session(pn_link_t *link);

/**
 * Retrieve the first link that matches the given state mask.
 *
 * Examines the state of each link owned by the connection and returns
 * the first link that matches the given state mask. If state contains
 * both local and remote flags, then an exact match against those
 * flags is performed. If state contains only local or only remote
 * flags, then a match occurs if any of the local or remote flags are
 * set respectively.
 *
 * @param[in] connection to be searched for matching Links
 * @param[in] state mask to match
 * @return the first link owned by the connection that matches the
 * mask, else NULL if no links match
 */
PN_EXTERN pn_link_t *pn_link_head(pn_connection_t *connection, pn_state_t state);

/**
 * Retrieve the next link that matches the given state mask.
 *
 * When used with pn_link_head, the application can access all links
 * on the connection that match the given state. See pn_link_head for
 * description of match behavior.
 *
 * @param[in] link the previous link obtained from pn_link_head or
 *                 pn_link_next
 * @param[in] state mask to match
 * @return the next session owned by the connection that matches the
 * mask, else NULL if no sessions match
 */
PN_EXTERN pn_link_t *pn_link_next(pn_link_t *link, pn_state_t state);

/**
 * Open a link.
 *
 * Once this operation has completed, the PN_LOCAL_ACTIVE state flag
 * will be set.
 *
 * @param[in] link a link object
 */
PN_EXTERN void pn_link_open(pn_link_t *link);

/**
 * Close a link.
 *
 * Once this operation has completed, the PN_LOCAL_CLOSED state flag
 * will be set. This may be called without calling
 * ::pn_link_open, in this case it is equivalent to calling
 * ::pn_link_open followed by ::pn_link_close.
 *
 * @param[in] link a link object
 */
PN_EXTERN void pn_link_close(pn_link_t *link);

/**
 * Detach a link.
 *
 * @param[in] link a link object
 */
PN_EXTERN void pn_link_detach(pn_link_t *link);

/**
 * Access the locally defined source definition for a link.
 *
 * The pointer returned by this operation is valid until the link
 * object is freed.
 *
 * @param[in] link a link object
 * @return a pointer to a source terminus
 */
PN_EXTERN pn_terminus_t *pn_link_source(pn_link_t *link);

/**
 * Access the locally defined target definition for a link.
 *
 * The pointer returned by this operation is valid until the link
 * object is freed.
 *
 * @param[in] link a link object
 * @return a pointer to a target terminus
 */
PN_EXTERN pn_terminus_t *pn_link_target(pn_link_t *link);

/**
 * Access the remotely defined source definition for a link.
 *
 * The pointer returned by this operation is valid until the link
 * object is freed. The remotely defined terminus will be empty until
 * the link is remotely opened as indicated by the PN_REMOTE_ACTIVE
 * flag.
 *
 * @param[in] link a link object
 * @return a pointer to the remotely defined source terminus
 */
PN_EXTERN pn_terminus_t *pn_link_remote_source(pn_link_t *link);

/**
 * Access the remotely defined target definition for a link.
 *
 * The pointer returned by this operation is valid until the link
 * object is freed. The remotely defined terminus will be empty until
 * the link is remotely opened as indicated by the PN_REMOTE_ACTIVE
 * flag.
 *
 * @param[in] link a link object
 * @return a pointer to the remotely defined target terminus
 */
PN_EXTERN pn_terminus_t *pn_link_remote_target(pn_link_t *link);

/**
 * Get the current delivery for a link.
 *
 * Each link maintains a sequence of deliveries in the order they were
 * created, along with a pointer to the *current* delivery. All
 * send/recv operations on a link take place on the *current*
 * delivery. If a link has no current delivery, the current delivery
 * is automatically initialized to the next delivery created on the
 * link. Once initialized, the current delivery remains the same until
 * it is changed through use of ::pn_link_advance or until it is
 * settled via ::pn_delivery_settle.
 *
 * @param[in] link a link object
 * @return the current delivery for the link, or NULL if there is none
 */
PN_EXTERN pn_delivery_t *pn_link_current(pn_link_t *link);

/**
 * Advance the current delivery of a link to the next delivery on the
 * link.
 *
 * For sending links this operation is used to finish sending message
 * data for the current outgoing delivery and move on to the next
 * outgoing delivery (if any).
 *
 * For receiving links, this operation is used to finish accessing
 * message data from the current incoming delivery and move on to the
 * next incoming delivery (if any).
 *
 * Each link maintains a sequence of deliveries in the order they were
 * created, along with a pointer to the *current* delivery. The
 * pn_link_advance operation will modify the *current* delivery on the
 * link to point to the next delivery in the sequence. If there is no
 * next delivery in the sequence, the current delivery will be set to
 * NULL. This operation will return true if invoking it caused the
 * value of the current delivery to change, even if it was set to
 * NULL.
 *
 * @param[in] link a link object
 * @return true if the current delivery was changed
 */
PN_EXTERN bool pn_link_advance(pn_link_t *link);

/**
 * Get the credit balance for a link.
 *
 * Links use a credit based flow control scheme. Every receiver
 * maintains a credit balance that corresponds to the number of
 * deliveries that the receiver can accept at any given moment. As
 * more capacity becomes available at the receiver (see
 * ::pn_link_flow), it adds credit to this balance and communicates
 * the new balance to the sender. Whenever a delivery is
 * sent/received, the credit balance maintained by the link is
 * decremented by one. Once the credit balance at the sender reaches
 * zero, the sender must pause sending until more credit is obtained
 * from the receiver.
 *
 * Note that a sending link may still be used to send deliveries even
 * if pn_link_credit reaches zero, however those deliveries will end
 * up being buffered by the link until enough credit is obtained from
 * the receiver to send them over the wire. In this case the balance
 * reported by ::pn_link_credit will go negative.
 *
 * @param[in] link a link object
 * @return the credit balance for the link
 */
PN_EXTERN int pn_link_credit(pn_link_t *link);

/**
 * Get the number of queued deliveries for a link.
 *
 * Links may queue deliveries for a number of reasons, for example
 * there may be insufficient credit to send them to the receiver (see
 * ::pn_link_credit), or they simply may not have yet had a chance to
 * be written to the wire. This operation will return the number of
 * queued deliveries on a link.
 *
 * @param[in] link a link object
 * @return the number of queued deliveries for the link
 */
PN_EXTERN int pn_link_queued(pn_link_t *link);

/**
 * Get the remote view of the credit for a link.
 *
 * The remote view of the credit for a link differs from local view of
 * credit for a link by the number of queued deliveries. In other
 * words ::pn_link_remote_credit is defined to be ::pn_link_credit -
 * ::pn_link_queued.
 *
 * @param[in] link a link object
 * @return the remote view of the credit for a link
 */
PN_EXTERN int pn_link_remote_credit(pn_link_t *link);

/**
 * Get the drain flag for a link.
 *
 * If a link is in drain mode, then the sending endpoint of a link
 * must immediately use up all available credit on the link. If this
 * is not possible, the excess credit must be returned by invoking
 * ::pn_link_drained. Only the receiving endpoint can set the drain
 * mode. See ::pn_link_set_drain for details.
 *
 * @param[in] link a link object
 * @return true if and only if the link is in drain mode
 */
PN_EXTERN bool pn_link_get_drain(pn_link_t *link);

/**
 * Drain excess credit for a link.
 *
 * When a link is in drain mode, the sender must use all excess credit
 * immediately, and release any excess credit back to the receiver if
 * there are no deliveries available to send.
 *
 * When invoked on a sending link that is in drain mode, this
 * operation will release all excess credit back to the receiver and
 * return the number of credits released back to the sender. If the
 * link is not in drain mode, this operation is a noop.
 *
 * When invoked on a receiving link, this operation will return and
 * reset the number of credits the sender has released back to the
 * receiver.
 *
 * @param[in] link a link object
 * @return the number of credits drained
 */
PN_EXTERN int pn_link_drained(pn_link_t *link);

/**
 * Get the available deliveries hint for a link.
 *
 * The available count for a link provides a hint as to the number of
 * deliveries that might be able to be sent if sufficient credit were
 * issued by the receiving link endpoint. See ::pn_link_offered for
 * more details.
 *
 * @param[in] link a link object
 * @return the available deliveries hint
 */
PN_EXTERN int pn_link_available(pn_link_t *link);

/**
 * Describes the permitted/expected settlement behaviours of a sending
 * link.
 *
 * The sender settle mode describes the permitted and expected
 * behaviour of a sending link with respect to settling of deliveries.
 * See ::pn_delivery_settle for more details.
 */
typedef enum {
  PN_SND_UNSETTLED = 0, /**< The sender will send all deliveries
                           initially unsettled. */
  PN_SND_SETTLED = 1, /**< The sender will send all deliveries settled
                         to the receiver. */
  PN_SND_MIXED = 2 /**< The sender may send a mixure of settled and
                      unsettled deliveries. */
} pn_snd_settle_mode_t;

/**
 * Describes the permitted/expected settlement behaviours of a
 * receiving link.
 *
 * The receiver settle mode describes the permitted and expected
 * behaviour of a receiving link with respect to settling of
 * deliveries. See ::pn_delivery_settle for more details.
 */
typedef enum {
  PN_RCV_FIRST = 0,  /**< The receiver will settle deliveries
                        regardless of what the sender does. */
  PN_RCV_SECOND = 1  /**< The receiver will only settle deliveries
                        after the sender settles. */
} pn_rcv_settle_mode_t;

/**
 * Get the local sender settle mode for a link.
 *
 * @param[in] link a link object
 * @return the local sender settle mode
 */
PN_EXTERN pn_snd_settle_mode_t pn_link_snd_settle_mode(pn_link_t *link);

/**
 * Get the local receiver settle mode for a link.
 *
 * @param[in] link a link object
 * @return the local receiver settle mode
 */
PN_EXTERN pn_rcv_settle_mode_t pn_link_rcv_settle_mode(pn_link_t *link);

/**
 * Set the local sender settle mode for a link.
 *
 * @param[in] link a link object
 * @param[in] mode the sender settle mode
 */
PN_EXTERN void pn_link_set_snd_settle_mode(pn_link_t *link, pn_snd_settle_mode_t mode);

/**
 * Set the local receiver settle mode for a link.
 *
 * @param[in] link a link object
 * @param[in] mode the receiver settle mode
 */
PN_EXTERN void pn_link_set_rcv_settle_mode(pn_link_t *link, pn_rcv_settle_mode_t mode);

/**
 * Get the remote sender settle mode for a link.
 *
 * @param[in] link a link object
 * @return the remote sender settle mode
 */
PN_EXTERN pn_snd_settle_mode_t pn_link_remote_snd_settle_mode(pn_link_t *link);

/**
 * Get the remote receiver settle mode for a link.
 *
 * @param[in] link a link object
 * @return the remote receiver settle mode
 */
PN_EXTERN pn_rcv_settle_mode_t pn_link_remote_rcv_settle_mode(pn_link_t *link);

/**
 * Get the number of unsettled deliveries for a link.
 *
 * @param[in] link a link object
 * @return the number of unsettled deliveries
 */
PN_EXTERN int pn_link_unsettled(pn_link_t *link);

/**
 * Get the first unsettled delivery for a link.
 *
 " @param[in] link a link object
 * @return a pointer to the first unsettled delivery on the link
 */
PN_EXTERN pn_delivery_t *pn_unsettled_head(pn_link_t *link);

/**
 * Get the next unsettled delivery on a link.
 *
 * @param[in] delivery a delivery object
 * @return the next unsettled delivery on the link
 */
PN_EXTERN pn_delivery_t *pn_unsettled_next(pn_delivery_t *delivery);

/**
 * @defgroup sender Sender
 * @{
 */

/**
 * Signal the availability of deliveries for a link.
 *
 * @param[in] sender a sender link object
 * @param[in] credit the number of deliveries potentially available
 * for transfer
 */
PN_EXTERN void pn_link_offered(pn_link_t *sender, int credit);

/**
 * Send message data for the current delivery on a link.
 *
 * @param[in] sender a sender link object
 * @param[in] bytes the start of the message data
 * @param[in] n the number of bytes of message data
 * @return the number of bytes sent, or an error code
 */
PN_EXTERN ssize_t pn_link_send(pn_link_t *sender, const char *bytes, size_t n);

//PN_EXTERN void pn_link_abort(pn_sender_t *sender);

/** @} */

// receiver
/**
 * @defgroup receiver Receiver
 * @{
 */

/**
 * Grant credit for incoming deliveries on a receiver.
 *
 * @param[in] receiver a receiving link object
 * @param[in] credit the amount to increment the link credit
 */
PN_EXTERN void pn_link_flow(pn_link_t *receiver, int credit);

/**
 * Grant credit for incoming deliveries on a receiver, and set drain
 * mode to true.
 *
 * Use ::pn_link_set_drain to set the drain mode explicitly.
 *
 * @param[in] receiver a receiving link object
 * @param[in] credit the amount to increment the link credit
 */
PN_EXTERN void pn_link_drain(pn_link_t *receiver, int credit);

/**
 * Set the drain mode on a link.
 *
 * @param[in] receiver a receiving link object
 * @param[in] drain the drain mode
 */
PN_EXTERN void pn_link_set_drain(pn_link_t *receiver, bool drain);

/**
 * Receive message data for the current delivery on a link.
 *
 * Use ::pn_delivery_pending on the current delivery to figure out how
 * much buffer space is needed.
 *
 * Note that the link API can be used to stream large messages across
 * the network, so just because there is no data to read does not
 * imply the message is complete. To ensure the entirety of the
 * message data has been read, either invoke ::pn_link_recv until
 * PN_EOS is returned, or verify that ::pn_delivery_partial is false,
 * and ::pn_delivery_pending is 0.
 *
 * @param[in] receiver a receiving link object
 * @param[in] bytes a pointer to an empty buffer
 * @param[in] n the buffer capacity
 * @return the number of bytes received, PN_EOS, or an error code
 */
PN_EXTERN ssize_t pn_link_recv(pn_link_t *receiver, char *bytes, size_t n);

/**
 * Check if a link is currently draining.
 *
 * A link is defined to be draining when drain mode is set to true,
 * and the sender still has excess credit.
 *
 * @param[in] receiver a receiving link object
 * @return true if the link is currently draining, false otherwise
 */
PN_EXTERN bool pn_link_draining(pn_link_t *receiver);

/** @} */

/** @}
 */

#ifdef __cplusplus
}
#endif

#endif /* link.h */

