#ifndef PROTON_ENGINE_H
#define PROTON_ENGINE_H 1

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
#ifndef __cplusplus
#include <stdbool.h>
#endif
#include <stddef.h>
#include <sys/types.h>
#include <proton/codec.h>
#include <proton/error.h>

#ifdef __cplusplus
extern "C" {
#endif

/** @file
 * API for the proton Engine.
 *
 * @todo
 */

typedef struct pn_transport_t pn_transport_t;
typedef struct pn_connection_t pn_connection_t; /**< Connection */
typedef struct pn_session_t pn_session_t;       /**< Session */
typedef struct pn_link_t pn_link_t;             /**< Link */
typedef struct pn_terminus_t pn_terminus_t;
typedef struct pn_condition_t pn_condition_t;

typedef enum {
  PN_UNSPECIFIED = 0,
  PN_SOURCE = 1,
  PN_TARGET = 2,
  PN_COORDINATOR = 3
} pn_terminus_type_t;
typedef enum {
  PN_NONDURABLE = 0,
  PN_CONFIGURATION = 1,
  PN_DELIVERIES = 2
} pn_durability_t;
typedef enum {
  PN_LINK_CLOSE,
  PN_SESSION_CLOSE,
  PN_CONNECTION_CLOSE,
  PN_NEVER
} pn_expiry_policy_t;
typedef enum {
  PN_DIST_MODE_UNSPECIFIED,
  PN_DIST_MODE_COPY,
  PN_DIST_MODE_MOVE
} pn_distribution_mode_t;
typedef enum {
  PN_SND_UNSETTLED = 0,
  PN_SND_SETTLED = 1,
  PN_SND_MIXED = 2
} pn_snd_settle_mode_t;
typedef enum {
  PN_RCV_FIRST = 0,  /**< implicitly settle rcvd xfers */
  PN_RCV_SECOND = 1  /**< explicit disposition required */
} pn_rcv_settle_mode_t;

typedef struct pn_disposition_t pn_disposition_t;
typedef struct pn_delivery_t pn_delivery_t;

typedef struct pn_delivery_tag_t {
  size_t size;
  const char *bytes;
} pn_delivery_tag_t;

#ifndef SWIG  // older versions of SWIG choke on this:
static inline pn_delivery_tag_t pn_dtag(const char *bytes, size_t size) {
  pn_delivery_tag_t dtag = {size, bytes};
  return dtag;
}
#endif

typedef int pn_state_t;     /**< encodes the state of an endpoint */

#define PN_LOCAL_UNINIT (1)    /**< local endpoint requires initialization */
#define PN_LOCAL_ACTIVE (2)    /**< local endpoint is active */
#define PN_LOCAL_CLOSED (4)    /**< local endpoint is closed */
#define PN_REMOTE_UNINIT (8)   /**< remote endpoint pending initialization by peer */
#define PN_REMOTE_ACTIVE (16)  /**< remote endpoint is active */
#define PN_REMOTE_CLOSED (32)  /**< remote endpoint has closed */

#define PN_LOCAL_MASK (PN_LOCAL_UNINIT | PN_LOCAL_ACTIVE | PN_LOCAL_CLOSED)
#define PN_REMOTE_MASK (PN_REMOTE_UNINIT | PN_REMOTE_ACTIVE | PN_REMOTE_CLOSED)

/**
 * The state/outcome of a message transfer.
 *
 * @todo document each value
 */

#define PN_RECEIVED (0x0000000000000023)
#define PN_ACCEPTED (0x0000000000000024)
#define PN_REJECTED (0x0000000000000025)
#define PN_RELEASED (0x0000000000000026)
#define PN_MODIFIED (0x0000000000000027)

typedef int pn_trace_t;
typedef void (pn_tracer_t)(pn_transport_t *transport, const char *message);

#define PN_TRACE_OFF (0)
#define PN_TRACE_RAW (1)
#define PN_TRACE_FRM (2)
#define PN_TRACE_DRV (4)

// connection

/** Factory to construct a new Connection.
 *
 * @return pointer to a new connection object.
 */
PN_EXTERN pn_connection_t *pn_connection(void);

/** Retrieve the state of the connection.
 *
 * @param[in] connection the connection
 * @return the connection's state flags
 */
PN_EXTERN pn_state_t pn_connection_state(pn_connection_t *connection);
/** @todo: needs documentation */
PN_EXTERN pn_error_t *pn_connection_error(pn_connection_t *connection);
/** @todo: needs documentation */
PN_EXTERN const char *pn_connection_get_container(pn_connection_t *connection);
/** @todo: needs documentation */
PN_EXTERN void pn_connection_set_container(pn_connection_t *connection, const char *container);
/** @todo: needs documentation */
PN_EXTERN const char *pn_connection_get_hostname(pn_connection_t *connection);
/** @todo: needs documentation */
PN_EXTERN void pn_connection_set_hostname(pn_connection_t *connection, const char *hostname);
PN_EXTERN const char *pn_connection_remote_container(pn_connection_t *connection);
PN_EXTERN const char *pn_connection_remote_hostname(pn_connection_t *connection);
PN_EXTERN pn_data_t *pn_connection_offered_capabilities(pn_connection_t *connection);
PN_EXTERN pn_data_t *pn_connection_desired_capabilities(pn_connection_t *connection);
PN_EXTERN pn_data_t *pn_connection_properties(pn_connection_t *connection);
PN_EXTERN pn_data_t *pn_connection_remote_offered_capabilities(pn_connection_t *connection);
PN_EXTERN pn_data_t *pn_connection_remote_desired_capabilities(pn_connection_t *connection);
PN_EXTERN pn_data_t *pn_connection_remote_properties(pn_connection_t *connection);


/** Extracts the first delivery on the connection that has pending
 *  operations.
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

/** Get the next delivery on the connection that needs has pending
 *  operations.
 *
 * @param[in] delivery the previous delivery retrieved from
 *                     either pn_work_head() or pn_work_next()
 * @return the next delivery that has pending operations, else
 * NULL if none
 */
PN_EXTERN pn_delivery_t *pn_work_next(pn_delivery_t *delivery);

/** Factory for creating a new session on the connection.
 *
 * A new session is created for the connection, and is added to the
 * set of sessions maintained by the connection.
 *
 * @param[in] connection the session will exist over this connection
 * @return pointer to new session
 */
PN_EXTERN pn_session_t *pn_session(pn_connection_t *connection);

PN_EXTERN size_t pn_session_get_incoming_capacity(pn_session_t *ssn);
PN_EXTERN void pn_session_set_incoming_capacity(pn_session_t *ssn, size_t capacity);

PN_EXTERN size_t pn_session_outgoing_bytes(pn_session_t *ssn);
PN_EXTERN size_t pn_session_incoming_bytes(pn_session_t *ssn);

/** Factory for creating a transport.
 *
 * A transport to be used by a connection to interface with the
 * network. There can only be one connection associated with a
 * transport. See pn_transport_bind().
 *
 * @return pointer to new transport
 */
PN_EXTERN pn_transport_t *pn_transport(void);

/** Binds the transport to an AMQP connection endpoint.
 *
 * @return an error code, or 0 on success
 */

PN_EXTERN int pn_transport_bind(pn_transport_t *transport, pn_connection_t *connection);

PN_EXTERN int pn_transport_unbind(pn_transport_t *transport);

/** Retrieve the first Session that matches the given state mask.
 *
 * Examines the state of each session owned by the connection, and
 * returns the first Session that matches the given state mask. If
 * state contains both local and remote flags, then an exact match
 * against those flags is performed. If state contains only local or
 * only remote flags, then a match occurs if any of the local or
 * remote flags are set respectively.
 *
 * @param[in] connection to be searched for matching sessions
 * @param[in] state mask to match
 * @return the first session owned by the connection that matches the
 * mask, else NULL if no sessions match
 */
PN_EXTERN pn_session_t *pn_session_head(pn_connection_t *connection, pn_state_t state);

/** Retrieve the next Session that matches the given state mask.
 *
 * When used with pn_session_head(), application can access all
 * Sessions on the connection that match the given state. See
 * pn_session_head() for description of match behavior.
 *
 * @param[in] session the previous session obtained from
 *                    pn_session_head() or pn_session_next()
 * @param[in] state mask to match.
 * @return the next session owned by the connection that matches the
 * mask, else NULL if no sessions match
 */
PN_EXTERN pn_session_t *pn_session_next(pn_session_t *session, pn_state_t state);

/** Retrieve the first Link that matches the given state mask.
 *
 * Examines the state of each Link owned by the connection and returns
 * the first Link that matches the given state mask. If state contains
 * both local and remote flags, then an exact match against those
 * flags is performed. If state contains only local or only remote
 * flags, then a match occurs if any of the local or remote flags are
 * set respectively.
 *
 * @param[in] connection to be searched for matching Links
 * @param[in] state mask to match
 * @return the first Link owned by the connection that matches the
 * mask, else NULL if no Links match
 */
PN_EXTERN pn_link_t *pn_link_head(pn_connection_t *connection, pn_state_t state);

/** Retrieve the next Link that matches the given state mask.
 *
 * When used with pn_link_head(), the application can access all Links
 * on the connection that match the given state. See pn_link_head()
 * for description of match behavior.
 *
 * @param[in] link the previous Link obtained from pn_link_head() or
 *                 pn_link_next()
 * @param[in] state mask to match
 * @return the next session owned by the connection that matches the
 * mask, else NULL if no sessions match
 */
PN_EXTERN pn_link_t *pn_link_next(pn_link_t *link, pn_state_t state);

PN_EXTERN void pn_connection_reset(pn_connection_t *connection);
PN_EXTERN void pn_connection_open(pn_connection_t *connection);
PN_EXTERN void pn_connection_close(pn_connection_t *connection);
PN_EXTERN void pn_connection_free(pn_connection_t *connection);

/** Access the application context that is associated with the
 *  connection.
 *
 * @param[in] connection the connection whose context is to be returned.
 *
 * @return the application context that was passed to pn_connection_set_context()
 */
PN_EXTERN void *pn_connection_get_context(pn_connection_t *connection);

/** Assign a new application context to the connection.
 *
 * @param[in] connection the connection which will hold the context.
 * @param[in] context new application context to associate with the
 *                    connection
 */
PN_EXTERN void pn_connection_set_context(pn_connection_t *connection, void *context);


// transport
PN_EXTERN pn_error_t *pn_transport_error(pn_transport_t *transport);
/* deprecated */
PN_EXTERN ssize_t pn_transport_input(pn_transport_t *transport, const char *bytes, size_t available);
/* deprecated */
PN_EXTERN ssize_t pn_transport_output(pn_transport_t *transport, char *bytes, size_t size);

/** Report the amount of free space for input following the
 * transport's tail pointer. If the engine is in an exceptional state
 * such as encountering an error condition or reaching the end of
 * stream state, a negative value will be returned indicating the
 * condition. If an error is indicated, futher details can be obtained
 * from ::pn_transport_error. Calls to ::pn_transport_process may
 * alter the value of this pointer. See ::pn_transport_process for
 * details.
 *
 * @param[in] transport the transport
 * @return the free space in the transport, PN_EOS or error code if < 0
 */
PN_EXTERN ssize_t pn_transport_capacity(pn_transport_t *transport);

/** Return the transport's tail pointer. The amount of free space
 * following this pointer is reported by ::pn_transport_capacity.
 * Calls to ::pn_transport_process may alther the value of this
 * pointer. See ::pn_transport_process for details.
 *
 * @param[in] transport the transport
 * @return a pointer to the transport's input buffer, NULL if no capacity available.
 */
PN_EXTERN char *pn_transport_tail(pn_transport_t *transport);

/** Pushes the supplied bytes into the tail of the transport. This is
 * equivalent to copying ::size bytes afther the tail pointer and then
 * calling ::pn_transport_process with an argument of ::size. It is an
 * error to call this with a size larger than the capacity reported by
 * ::pn_transport_capacity.
 *
 * @param[in] transport the transport
 * @return 0 on success, or error code if < 0
 */
PN_EXTERN int pn_transport_push(pn_transport_t *transport, const char *src, size_t size);

/** Process input data following the tail pointer. Calling this
 * function will cause the transport to consume ::size bytes of input
 * occupying the free space following the tail pointer. Calls to this
 * function may change the value of ::pn_transport_tail, as well as
 * the amount of free space reported by ::pn_transport_capacity.
 *
 * @param[in] transport the transport
 * @param[size] the amount of data written to the transport's input buffer
 * @return 0 on success, or error code if < 0
 */
PN_EXTERN int pn_transport_process(pn_transport_t *transport, size_t size);

/** Indicate that the input has reached End Of Stream (EOS).  This
 * tells the transport that no more input will be forthcoming.
 *
 * @param[in] transport the transport
 * @return 0 on success, or error code if < 0
 */
PN_EXTERN int pn_transport_close_tail(pn_transport_t *transport);

/** Report the number of pending output bytes following the
 * transport's head pointer. If the engine is in an exceptional state
 * such as encountering an error condition or reaching the end of
 * stream state, a negative value will be returned indicating the
 * condition. If an error is indicated, further details can be
 * obtained from ::pn_transport_error. Calls to ::pn_transport_pop may
 * alter the value of this pointer. See ::pn_transport_pop for
 * details.
 *
 * @param[in] the transport
 * @return the number of pending output bytes, or an error code
 */
PN_EXTERN ssize_t pn_transport_pending(pn_transport_t *transport);

/** Return the transport's head pointer. This pointer references
 * queued output data. The ::pn_transport_pending function reports how
 * many bytes of output data follow this pointer. Calls to
 * ::pn_transport_pop may alter this pointer and any data it
 * references. See ::pn_transport_pop for details.
 *
 * @param[in] transport the transport
 * @return a pointer to the transport's output buffer, or NULL if no pending output.
 */
PN_EXTERN const char *pn_transport_head(pn_transport_t *transport);

/** Copies ::size bytes from the head of the transport to the ::dst
 * pointer. It is an error to call this with a value of ::size that is
 * greater than the value reported by ::pn_transport_pending.
 *
 * @param[in] transport the transport
 * @return 0 on success, or error code if < 0
 */
PN_EXTERN int pn_transport_peek(pn_transport_t *transport, char *dst, size_t size);

/** Removes ::size bytes of output from the pending output queue
 * following the transport's head pointer. Calls to this function may
 * alter the transport's head pointer as well as the number of pending
 * bytes reported by ::pn_transport_pending.
 *
 * @param[in] the transport
 * @param[size] the number of bytes to remove
 */
PN_EXTERN void pn_transport_pop(pn_transport_t *transport, size_t size);

/** Indicate that the output has closed.  This tells the transport
 * that no more output will be popped.
 *
 * @param[in] transport the transport
 * @return 0 on success, or error code if < 0
 */
PN_EXTERN int pn_transport_close_head(pn_transport_t *transport);


/** Process any pending transport timer events.
 *
 * This method should be called after all pending input has been processed by the
 * transport (see ::pn_transport_input), and before generating output (see
 * ::pn_transport_output).  It returns the deadline for the next pending timer event, if
 * any are present.
 *
 * @param[in] transport the transport to process.
 *
 * @return if non-zero, then the expiration time of the next pending timer event for the
 * transport.  The caller must invoke pn_transport_tick again at least once at or before
 * this deadline occurs.
 */
PN_EXTERN pn_timestamp_t pn_transport_tick(pn_transport_t *transport, pn_timestamp_t now);
PN_EXTERN void pn_transport_trace(pn_transport_t *transport, pn_trace_t trace);
PN_EXTERN void pn_transport_set_tracer(pn_transport_t *transport, pn_tracer_t *tracer);
PN_EXTERN pn_tracer_t *pn_transport_get_tracer(pn_transport_t *transport);
PN_EXTERN void pn_transport_set_context(pn_transport_t *transport, void *context);
PN_EXTERN void *pn_transport_get_context(pn_transport_t *transport);
PN_EXTERN void pn_transport_log(pn_transport_t *transport, const char *message);
PN_EXTERN void pn_transport_logf(pn_transport_t *transport, const char *fmt, ...);
// max frame of zero means "unlimited"
PN_EXTERN uint32_t pn_transport_get_max_frame(pn_transport_t *transport);
PN_EXTERN void pn_transport_set_max_frame(pn_transport_t *transport, uint32_t size);
PN_EXTERN uint32_t pn_transport_get_remote_max_frame(pn_transport_t *transport);
/* timeout of zero means "no timeout" */
PN_EXTERN pn_millis_t pn_transport_get_idle_timeout(pn_transport_t *transport);
PN_EXTERN void pn_transport_set_idle_timeout(pn_transport_t *transport, pn_millis_t timeout);
PN_EXTERN pn_millis_t pn_transport_get_remote_idle_timeout(pn_transport_t *transport);
PN_EXTERN uint64_t pn_transport_get_frames_output(const pn_transport_t *transport);
PN_EXTERN uint64_t pn_transport_get_frames_input(const pn_transport_t *transport);
PN_EXTERN bool pn_transport_quiesced(pn_transport_t *transport);
PN_EXTERN void pn_transport_free(pn_transport_t *transport);

// session
PN_EXTERN pn_state_t pn_session_state(pn_session_t *session);
PN_EXTERN pn_error_t *pn_session_error(pn_session_t *session);
PN_EXTERN pn_connection_t *pn_session_connection(pn_session_t *session);
PN_EXTERN void pn_session_open(pn_session_t *session);
PN_EXTERN void pn_session_close(pn_session_t *session);
PN_EXTERN void pn_session_free(pn_session_t *session);
PN_EXTERN void *pn_session_get_context(pn_session_t *session);
PN_EXTERN void pn_session_set_context(pn_session_t *session, void *context);

// link
PN_EXTERN pn_link_t *pn_sender(pn_session_t *session, const char *name);
PN_EXTERN pn_link_t *pn_receiver(pn_session_t *session, const char *name);
PN_EXTERN const char *pn_link_name(pn_link_t *link);
PN_EXTERN bool pn_link_is_sender(pn_link_t *link);
PN_EXTERN bool pn_link_is_receiver(pn_link_t *link);
PN_EXTERN pn_state_t pn_link_state(pn_link_t *link);
PN_EXTERN pn_error_t *pn_link_error(pn_link_t *link);
PN_EXTERN pn_session_t *pn_link_session(pn_link_t *link);
PN_EXTERN pn_terminus_t *pn_link_source(pn_link_t *link);
PN_EXTERN pn_terminus_t *pn_link_target(pn_link_t *link);
PN_EXTERN pn_terminus_t *pn_link_remote_source(pn_link_t *link);
PN_EXTERN pn_terminus_t *pn_link_remote_target(pn_link_t *link);
PN_EXTERN pn_delivery_t *pn_link_current(pn_link_t *link);
PN_EXTERN bool pn_link_advance(pn_link_t *link);
PN_EXTERN int pn_link_credit(pn_link_t *link);
PN_EXTERN int pn_link_queued(pn_link_t *link);
PN_EXTERN int pn_link_remote_credit(pn_link_t *link);
PN_EXTERN int pn_link_available(pn_link_t *link);
PN_EXTERN pn_snd_settle_mode_t pn_link_snd_settle_mode(pn_link_t *link);
PN_EXTERN pn_rcv_settle_mode_t pn_link_rcv_settle_mode(pn_link_t *link);
PN_EXTERN pn_snd_settle_mode_t pn_link_remote_snd_settle_mode(pn_link_t *link);
PN_EXTERN pn_rcv_settle_mode_t pn_link_remote_rcv_settle_mode(pn_link_t *link);
PN_EXTERN void pn_link_set_snd_settle_mode(pn_link_t *link, pn_snd_settle_mode_t);
PN_EXTERN void pn_link_set_rcv_settle_mode(pn_link_t *link, pn_rcv_settle_mode_t);

PN_EXTERN int pn_link_unsettled(pn_link_t *link);
PN_EXTERN pn_delivery_t *pn_unsettled_head(pn_link_t *link);
PN_EXTERN pn_delivery_t *pn_unsettled_next(pn_delivery_t *delivery);

PN_EXTERN void pn_link_open(pn_link_t *sender);
PN_EXTERN void pn_link_close(pn_link_t *sender);
PN_EXTERN void pn_link_free(pn_link_t *sender);
PN_EXTERN void *pn_link_get_context(pn_link_t *link);
PN_EXTERN void pn_link_set_context(pn_link_t *link, void *context);
PN_EXTERN bool pn_link_get_drain(pn_link_t *link);

// sender
PN_EXTERN void pn_link_offered(pn_link_t *sender, int credit);
PN_EXTERN ssize_t pn_link_send(pn_link_t *sender, const char *bytes, size_t n);
PN_EXTERN int pn_link_drained(pn_link_t *sender);
//void pn_link_abort(pn_sender_t *sender);

// receiver
PN_EXTERN void pn_link_flow(pn_link_t *receiver, int credit);
PN_EXTERN void pn_link_drain(pn_link_t *receiver, int credit);
PN_EXTERN void pn_link_set_drain(pn_link_t *receiver, bool drain);
PN_EXTERN ssize_t pn_link_recv(pn_link_t *receiver, char *bytes, size_t n);
PN_EXTERN bool pn_link_draining(pn_link_t *receiver);

// terminus
PN_EXTERN pn_terminus_type_t pn_terminus_get_type(pn_terminus_t *terminus);
PN_EXTERN int pn_terminus_set_type(pn_terminus_t *terminus, pn_terminus_type_t type);

PN_EXTERN const char *pn_terminus_get_address(pn_terminus_t *terminus);
PN_EXTERN int pn_terminus_set_address(pn_terminus_t *terminus, const char *address);
PN_EXTERN pn_durability_t pn_terminus_get_durability(pn_terminus_t *terminus);
PN_EXTERN int pn_terminus_set_durability(pn_terminus_t *terminus,
                               pn_durability_t durability);
PN_EXTERN pn_expiry_policy_t pn_terminus_get_expiry_policy(pn_terminus_t *terminus);
PN_EXTERN int pn_terminus_set_expiry_policy(pn_terminus_t *terminus, pn_expiry_policy_t policy);
PN_EXTERN pn_seconds_t pn_terminus_get_timeout(pn_terminus_t *terminus);
PN_EXTERN int pn_terminus_set_timeout(pn_terminus_t *terminus, pn_seconds_t);
PN_EXTERN bool pn_terminus_is_dynamic(pn_terminus_t *terminus);
PN_EXTERN int pn_terminus_set_dynamic(pn_terminus_t *terminus, bool dynamic);
PN_EXTERN pn_data_t *pn_terminus_properties(pn_terminus_t *terminus);
PN_EXTERN pn_data_t *pn_terminus_capabilities(pn_terminus_t *terminus);
PN_EXTERN pn_data_t *pn_terminus_outcomes(pn_terminus_t *terminus);
PN_EXTERN pn_data_t *pn_terminus_filter(pn_terminus_t *terminus);
PN_EXTERN pn_distribution_mode_t pn_terminus_get_distribution_mode(const pn_terminus_t *terminus);
PN_EXTERN int pn_terminus_set_distribution_mode(pn_terminus_t *terminus, pn_distribution_mode_t m);
PN_EXTERN int pn_terminus_copy(pn_terminus_t *terminus, pn_terminus_t *src);

// delivery
PN_EXTERN pn_delivery_t *pn_delivery(pn_link_t *link, pn_delivery_tag_t tag);
PN_EXTERN pn_delivery_tag_t pn_delivery_tag(pn_delivery_t *delivery);
PN_EXTERN pn_link_t *pn_delivery_link(pn_delivery_t *delivery);
// how do we do delivery state?
PN_EXTERN pn_disposition_t *pn_delivery_local(pn_delivery_t *delivery);
PN_EXTERN uint64_t pn_delivery_local_state(pn_delivery_t *delivery);
PN_EXTERN pn_disposition_t *pn_delivery_remote(pn_delivery_t *delivery);
PN_EXTERN uint64_t pn_delivery_remote_state(pn_delivery_t *delivery);
PN_EXTERN bool pn_delivery_settled(pn_delivery_t *delivery);
PN_EXTERN size_t pn_delivery_pending(pn_delivery_t *delivery);
PN_EXTERN bool pn_delivery_partial(pn_delivery_t *delivery);
PN_EXTERN bool pn_delivery_writable(pn_delivery_t *delivery);
PN_EXTERN bool pn_delivery_readable(pn_delivery_t *delivery);
PN_EXTERN bool pn_delivery_updated(pn_delivery_t *delivery);
PN_EXTERN void pn_delivery_update(pn_delivery_t *delivery, uint64_t state);
PN_EXTERN void pn_delivery_clear(pn_delivery_t *delivery);
//int pn_delivery_format(pn_delivery_t *delivery);
PN_EXTERN void pn_delivery_settle(pn_delivery_t *delivery);
PN_EXTERN void pn_delivery_dump(pn_delivery_t *delivery);
PN_EXTERN void *pn_delivery_get_context(pn_delivery_t *delivery);
PN_EXTERN void pn_delivery_set_context(pn_delivery_t *delivery, void *context);
PN_EXTERN bool pn_delivery_buffered(pn_delivery_t *delivery);

// disposition
PN_EXTERN uint64_t pn_disposition_type(pn_disposition_t *disposition);
PN_EXTERN pn_data_t *pn_disposition_data(pn_disposition_t *disposition);
PN_EXTERN uint32_t pn_disposition_get_section_number(pn_disposition_t *disposition);
PN_EXTERN void pn_disposition_set_section_number(pn_disposition_t *disposition, uint32_t section_number);
PN_EXTERN uint64_t pn_disposition_get_section_offset(pn_disposition_t *disposition);
PN_EXTERN void pn_disposition_set_section_offset(pn_disposition_t *disposition, uint64_t section_offset);
PN_EXTERN bool pn_disposition_is_failed(pn_disposition_t *disposition);
PN_EXTERN void pn_disposition_set_failed(pn_disposition_t *disposition, bool failed);
PN_EXTERN bool pn_disposition_is_undeliverable(pn_disposition_t *disposition);
PN_EXTERN void pn_disposition_set_undeliverable(pn_disposition_t *disposition, bool undeliverable);
PN_EXTERN pn_data_t *pn_disposition_annotations(pn_disposition_t *disposition);

// conditions
PN_EXTERN pn_condition_t *pn_connection_condition(pn_connection_t *connection);
PN_EXTERN pn_condition_t *pn_connection_remote_condition(pn_connection_t *connection);

PN_EXTERN pn_condition_t *pn_session_condition(pn_session_t *session);
PN_EXTERN pn_condition_t *pn_session_remote_condition(pn_session_t *session);

PN_EXTERN pn_condition_t *pn_link_condition(pn_link_t *link);
PN_EXTERN pn_condition_t *pn_link_remote_condition(pn_link_t *link);

PN_EXTERN pn_condition_t *pn_disposition_condition(pn_disposition_t *disposition);

PN_EXTERN bool pn_condition_is_set(pn_condition_t *condition);
PN_EXTERN void pn_condition_clear(pn_condition_t *condition);

PN_EXTERN const char *pn_condition_get_name(pn_condition_t *condition);
PN_EXTERN int pn_condition_set_name(pn_condition_t *condition, const char *name);

PN_EXTERN const char *pn_condition_get_description(pn_condition_t *condition);
PN_EXTERN int pn_condition_set_description(pn_condition_t *condition, const char *description);

PN_EXTERN pn_data_t *pn_condition_info(pn_condition_t *condition);

PN_EXTERN bool pn_condition_is_redirect(pn_condition_t *condition);
PN_EXTERN const char *pn_condition_redirect_host(pn_condition_t *condition);
PN_EXTERN int pn_condition_redirect_port(pn_condition_t *condition);

#ifdef __cplusplus
}
#endif

#endif /* engine.h */
