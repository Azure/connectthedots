#ifndef PROTON_TYPES_H
#define PROTON_TYPES_H 1

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
#include <stddef.h>
#include <sys/types.h>
#include <proton/type_compat.h>

/**
 * @file
 *
 * @defgroup types Types
 * @{
 */

#ifdef __cplusplus
extern "C" {
#endif

/**
 * @defgroup primitives Primitive Types
 * @{
 */

typedef int32_t  pn_sequence_t;
typedef uint32_t pn_millis_t;
typedef uint32_t pn_seconds_t;
typedef int64_t  pn_timestamp_t;
typedef uint32_t pn_char_t;
typedef uint32_t pn_decimal32_t;
typedef uint64_t pn_decimal64_t;
typedef struct {
  char bytes[16];
} pn_decimal128_t;
typedef struct {
  char bytes[16];
} pn_uuid_t;

typedef struct {
  size_t size;
  const char *start;
} pn_bytes_t;

PN_EXTERN pn_bytes_t pn_bytes(size_t size, const char *start);

/** @}
 */

/**
 * @defgroup abstract Abstract Types
 * @{
 */

/**
 * Holds the state flags for an AMQP endpoint.
 *
 * A pn_state_t is an integral value with flags that encode both the
 * local and remote state of an AMQP Endpoint (@link pn_connection_t
 * Connection @endlink, @link pn_session_t Session @endlink, or @link
 * pn_link_t Link @endlink). The local portion of the state may be
 * accessed using ::PN_LOCAL_MASK, and the remote portion may be
 * accessed using ::PN_REMOTE_MASK. Individual bits may be accessed
 * using ::PN_LOCAL_UNINIT, ::PN_LOCAL_ACTIVE, ::PN_LOCAL_CLOSED, and
 * ::PN_REMOTE_UNINIT, ::PN_REMOTE_ACTIVE, ::PN_REMOTE_CLOSED.
 *
 * Every AMQP endpoint (@link pn_connection_t Connection @endlink,
 * @link pn_session_t Session @endlink, or @link pn_link_t Link
 * @endlink) starts out in an uninitialized state and then proceeds
 * linearly to an active and then closed state. This lifecycle occurs
 * at both endpoints involved, and so the state model for an endpoint
 * includes not only the known local state, but also the last known
 * state of the remote endpoint.
 *
 * @ingroup connection
 */
typedef int pn_state_t;

/**
 * An AMQP Connection object.
 *
 * A pn_connection_t object encapsulates all of the endpoint state
 * associated with an AMQP Connection. A pn_connection_t object
 * contains zero or more ::pn_session_t objects, which in turn contain
 * zero or more ::pn_link_t objects. Each ::pn_link_t object contains
 * an ordered sequence of ::pn_delivery_t objects. A link is either a
 * @link sender Sender @endlink, or a @link receiver Receiver
 * @endlink, but never both.
 *
 * @ingroup connection
 */
typedef struct pn_connection_t pn_connection_t;

/**
 * An AMQP Session object.
 *
 * A pn_session_t object encapsulates all of the endpoint state
 * associated with an AMQP Session. A pn_session_t object contains
 * zero or more ::pn_link_t objects.
 *
 * @ingroup session
 */
typedef struct pn_session_t pn_session_t;

/**
 * An AMQP Link object.
 *
 * A pn_link_t object encapsulates all of the endpoint state
 * associated with an AMQP Link. A pn_link_t object contains an
 * ordered sequence of ::pn_delivery_t objects representing in-flight
 * deliveries. A pn_link_t may be either a @link sender Sender
 * @endlink, or a @link receiver Receiver @endlink, but never both.
 *
 * A pn_link_t object maintains a pointer to the *current* delivery
 * within the ordered sequence of deliveries contained by the link
 * (See ::pn_link_current). The *current* delivery is the target of a
 * number of operations associated with the link, such as sending
 * (::pn_link_send) and receiving (::pn_link_recv) message data.
 *
 * @ingroup link
 */
typedef struct pn_link_t pn_link_t;

/**
 * An AMQP Delivery object.
 *
 * A pn_delivery_t object encapsulates all of the endpoint state
 * associated with an AMQP Delivery. Every delivery exists within the
 * context of a ::pn_link_t object.
 *
 * The AMQP model for settlement is based on the lifecycle of a
 * delivery at an endpoint. At each end of a link, a delivery is
 * created, it exists for some period of time, and finally it is
 * forgotten, aka settled. Note that because this lifecycle happens
 * independently at both the sender and the receiver, there are
 * actually four events of interest in the combined lifecycle of a
 * given delivery:
 *
 *   - created at sender
 *   - created at receiver
 *   - settled at sender
 *   - settled at receiver
 *
 * Because the sender and receiver are operating concurrently, these
 * events can occur in a variety of different orders, and the order of
 * these events impacts the types of failures that may occur when
 * transferring a delivery. Eliminating scenarios where the receiver
 * creates the delivery first, we have the following possible
 * sequences of interest:
 *
 * Sender presettles (aka at-most-once):
 * -------------------------------------
 *
 *   1. created at sender
 *   2. settled at sender
 *   3. created at receiver
 *   4. settled at receiver
 *
 * In this configuration the sender settles (i.e. forgets about) the
 * delivery before it even reaches the receiver, and if anything
 * should happen to the delivery in-flight, there is no way to
 * recover, hence the "at most once" semantics.
 *
 * Receiver settles first (aka at-least-once):
 * -------------------------------------------
 *
 *   1. created at sender
 *   2. created at receiver
 *   3. settled at receiver
 *   4. settled at sender
 *
 * In this configuration the receiver settles the delivery first, and
 * the sender settles once it sees the receiver has settled. Should
 * anything happen to the delivery in-flight, the sender can resend,
 * however the receiver may have already forgotten the delivery and so
 * it could interpret the resend as a new delivery, hence the "at
 * least once" semantics.
 *
 * Receiver settles second (aka exactly-once):
 * -------------------------------------------
 *
 *   1. created at sender
 *   2. created at receiver
 *   3. settled at sender
 *   4. settled at receiver
 *
 * In this configuration the receiver settles only once it has seen
 * that the sender has settled. This provides the sender the option to
 * retransmit, and the receiver has the option to recognize (and
 * discard) duplicates, allowing for exactly once semantics.
 *
 * Note that in the last scenario the sender needs some way to know
 * when it is safe to settle. This is where delivery state comes in.
 * In addition to these lifecycle related events surrounding
 * deliveries there is also the notion of a delivery state that can
 * change over the lifetime of a delivery, e.g. it might start out as
 * nothing, transition to ::PN_RECEIVED and then transition to
 * ::PN_ACCEPTED. In the first two scenarios the delivery state isn't
 * required, however in final scenario the sender would typically
 * trigger settlement based on seeing the delivery state transition to
 * a terminal state like ::PN_ACCEPTED or ::PN_REJECTED.
 *
 * In practice settlement is controlled by application policy, so
 * there may well be more options here, e.g. a sender might not settle
 * strictly based on what has happened at the receiver, it might also
 * choose to impose some time limit and settle after that period has
 * expired, or it could simply have a sliding window of the last N
 * deliveries and settle the oldest whenever a new one comes along.
 *
 * @ingroup delivery
 */
typedef struct pn_delivery_t pn_delivery_t;

/**
 * An event collector.
 *
 * A pn_collector_t may be used to register interest in being notified
 * of high level events that can occur to the various objects
 * representing AMQP endpoint state. See ::pn_event_t for more
 * details.
 *
 * @ingroup event
 */
typedef struct pn_collector_t pn_collector_t;

/**
 * An AMQP Transport object.
 *
 * A pn_transport_t encapsulates the transport related state of all
 * AMQP endpoint objects associated with a physical network connection
 * at a given point in time.
 *
 * @ingroup transport
 */

typedef struct pn_transport_t pn_transport_t;

/** @}
 */
#ifdef __cplusplus
}
#endif

/** @}
 */

#endif /* types.h */
