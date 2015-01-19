#ifndef PROTON_MESSENGER_H
#define PROTON_MESSENGER_H 1

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
#include <proton/message.h>

#ifdef __cplusplus
extern "C" {
#endif

/** @file
 * The messenger API provides a high level interface for sending and
 * receiving AMQP messages.
 */

typedef struct pn_messenger_t pn_messenger_t; /**< Messenger*/
typedef struct pn_subscription_t pn_subscription_t; /**< Subscription*/
typedef int64_t pn_tracker_t;

typedef enum {
  PN_STATUS_UNKNOWN = 0,
  PN_STATUS_PENDING = 1,
  PN_STATUS_ACCEPTED = 2,
  PN_STATUS_REJECTED = 3,
  PN_STATUS_RELEASED = 4,
  PN_STATUS_MODIFIED = 5,
  PN_STATUS_ABORTED = 6,
  PN_STATUS_SETTLED = 7
} pn_status_t;

/** Construct a new Messenger with the given name. The name is global.
 * If a NULL name is supplied, a UUID based name will be chosen.
 *
 * @param[in] name the name of the messenger or NULL
 *
 * @return pointer to a new Messenger
 */
PN_EXTERN pn_messenger_t *pn_messenger(const char *name);

/** Retrieves the name of a Messenger.
 *
 * @param[in] messenger the messenger
 *
 * @return the name of the messenger
 */
PN_EXTERN const char *pn_messenger_name(pn_messenger_t *messenger);

/** Sets the path that will be used to get the certificate
 * that will be used to identify this messenger to its
 * peers.  The validity of the path is not checked by
 * this function.
 *
 * @param[in] messenger the messenger
 * @param[in] certificate a path to a certificate file
 *
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_certificate(pn_messenger_t *messenger, const char *certificate);

/** Return the certificate path. This value may be set by
 * pn_messenger_set_certificate. The default certificate
 * path is null.
 *
 * @param[in] messenger the messenger
 * @return the certificate file path
 */
PN_EXTERN const char *pn_messenger_get_certificate(pn_messenger_t *messenger);

/** Provides the private key that was used to sign the certificate.
 * See ::pn_messenger_set_certificate
 *
 * @param[in] messenger the Messenger
 * @param[in] private_key a path to a private key file
 *
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_private_key(pn_messenger_t *messenger, const char *private_key);

/** Gets the private key file for a Messenger.
 *
 * @param[in] messenger the messenger
 * @return the private key file path
 */
PN_EXTERN const char *pn_messenger_get_private_key(pn_messenger_t *messenger);

/** Sets the private key password for a Messenger.
 *
 * @param[in] messenger the messenger
 * @param[in] password the password for the private key file
 *
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_password(pn_messenger_t *messenger, const char *password);

/** Gets the private key file password for a Messenger.
 *
 * @param[in] messenger the messenger
 * @return password for the private key file
 */
PN_EXTERN const char *pn_messenger_get_password(pn_messenger_t *messenger);

/** Sets the trusted certificates database for a Messenger.  Messenger
 * will use this database to validate the certificate provided by the
 * peer.
 *
 * @param[in] messenger the messenger
 * @param[in] cert_db a path to the certificates database
 *
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_trusted_certificates(pn_messenger_t *messenger, const char *cert_db);

/** Gets the trusted certificates database for a Messenger.
 *
 * @param[in] messenger the messenger
 * @return path to the trusted certificates database
 */
PN_EXTERN const char *pn_messenger_get_trusted_certificates(pn_messenger_t *messenger);

/** Any messenger call that blocks during execution will stop
 * blocking and return control when this timeout is reached,
 * if you have set it to a value greater than zero.
 * Expressed in milliseconds.
 *
 * @param[in] messenger the messenger
 * @param[in] timeout the new timeout for the messenger, in milliseconds
 *
 * @return an error code or zero if there is no error
 */
PN_EXTERN int pn_messenger_set_timeout(pn_messenger_t *messenger, int timeout);

/** Retrieves the timeout for a Messenger.
 *
 * @param[in] messenger the messenger
 *
 * @return the timeout for the messenger, in milliseconds
 */
PN_EXTERN int pn_messenger_get_timeout(pn_messenger_t *messenger);

/** Accessor for messenger blocking mode.
 *
 * @param[in] messenger the messenger
 *
 * @return true if blocking has been enabled.
 */
PN_EXTERN bool pn_messenger_is_blocking(pn_messenger_t *messenger);

/** Enable or disable blocking behavior during calls to
 * pn_messenger_send and pn_messenger_recv.
 *
 * @param[in] messenger the messenger
 *
 * @return true if blocking has been enabled.
 */
PN_EXTERN int pn_messenger_set_blocking(pn_messenger_t *messenger, bool blocking);

/** Frees a Messenger.
 *
 * @param[in] messenger the messenger to free, no longer valid on
 *                      return
 */
PN_EXTERN void pn_messenger_free(pn_messenger_t *messenger);

/** Return the code for the most recent error,
 * initialized to zero at messenger creation.
 * The error number is "sticky" i.e. are not reset to 0
 * at the end of successful API calls.
 *
 * @param[in] messenger the messenger to check for errors
 *
 * @return an error code or zero if there is no error
 * @see error.h
 */
PN_EXTERN int pn_messenger_errno(pn_messenger_t *messenger);

/** Returns a pointer to a pn_error_t. The pn_error_* API
 * allows you to access the text, error number, and lets you
 * set or clear the error code explicitly.
 *
 * @param[in] messenger the messenger to check for errors
 *
 * @return a pointer to the messenger's error descriptor
 * @see error.h
 */
PN_EXTERN pn_error_t *pn_messenger_error(pn_messenger_t *messenger);

/** Returns the size of the incoming window that was
 * set with pn_messenger_set_incoming_window.  The
 * default is 0.
 *
 * @param[in] messenger the messenger
 *
 * @return the outgoing window
 */
PN_EXTERN int pn_messenger_get_outgoing_window(pn_messenger_t *messenger);

/** The size of the outgoing window limits the number of messages whose
 * status you can check with a tracker. A message enters this window
 * when you call pn_messenger_put on the message.  If your outgoing window
 * size is 10, and you call pn_messenger_put 12, new status information
 * will no longer be available for the first 2 messages.
 *
 * @param[in] messenger the Messenger
 * @param[in] window the number of deliveries to track
 *
 * @return an error or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_set_outgoing_window(pn_messenger_t *messenger, int window);

/** Returns the size of the incoming window that was
 * set with pn_messenger_set_incoming_window.
 * The default is 0.
 *
 * @param[in] messenger the Messenger
 *
 * @return the incoming window
 */
PN_EXTERN int pn_messenger_get_incoming_window(pn_messenger_t *messenger);

/** The size of your incoming window limits the number of messages
 * that can be accepted or rejected using trackers.  Messages do
 * not enter this window when they have been received (pn_messenger_recv)
 * onto you incoming queue.  
 * 
 * Messages enter this window only when you
 * take them into your application using pn_messenger_get.
 * If your incoming window size is N, and you get N+1 messages without
 * explicitly accepting or rejecting the oldest message, then it will be
 * implicitly accepted when it falls off the edge of the incoming window.
 * 
 * @param[in] messenger the Messenger
 * @param[in] window the number of deliveries to track
 *
 * @return an error or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_set_incoming_window(pn_messenger_t *messenger, int window);

/** Currently a no-op placeholder.
 * For future compatibility, do not send or receive messages
 * before starting the messenger.
 *
 * @param[in] messenger the messenger to start
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_start(pn_messenger_t *messenger);

/** Stops a messenger.  A messenger cannot send or
 * receive messages after it is stopped.  The messenger may require
 * some time to stop if it is busy, and in that case will return
 * PN_INPROGRESS.  In that case, call pn_messenger_stopped to see
 * if it has fully stopped.
 *
 * @param[in] messenger the messenger to stop
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_stop(pn_messenger_t *messenger);

/** Returns true if a messenger is in the stopped state.
 * This function does not block.
 *
 * @param[in] messenger the messenger to stop
 *
 */
PN_EXTERN bool pn_messenger_stopped(pn_messenger_t *messenger);

/** Subscribes a messenger to messages from the specified source.
 *
 * @param[in] messenger the messenger to subscribe
 * @param[in] source
 *
 * @return a subscription
 */
PN_EXTERN pn_subscription_t *pn_messenger_subscribe(pn_messenger_t *messenger, const char *source);

PN_EXTERN void *pn_subscription_get_context(pn_subscription_t *sub);

PN_EXTERN void pn_subscription_set_context(pn_subscription_t *sub, void *context);

PN_EXTERN const char *pn_subscription_address(pn_subscription_t *sub);

/** Puts the message onto the messenger's outgoing queue.
 * The message may also be sent if transmission would not cause
 * blocking.  This call will not block.
 *
 * @param[in] messenger the messenger
 * @param[in] msg the message to put on the outgoing queue
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_put(pn_messenger_t *messenger, pn_message_t *msg);

/** Find the current delivery status of the outgoing message
 * associated with this tracker, as long as the message is still
 * within your outgoing window.within your outgoing window.
 *
 * @param[in] messenger the messenger
 * @param[in] tracker the tracker identifying the delivery
 *
 * @return a status code for the delivery
 */
PN_EXTERN pn_status_t pn_messenger_status(pn_messenger_t *messenger, pn_tracker_t tracker);

/** Checks if the delivery associated with the given tracker is still
 * waiting to be sent.
 *
 * @param[in] messenger the messenger
 * @param[in] tracker the tracker identifying the delivery
 *
 * @return true if delivery is still buffered
 */
PN_EXTERN bool pn_messenger_buffered(pn_messenger_t *messenger, pn_tracker_t tracker);

/** Frees a Messenger from tracking the status associated with a given
 * tracker. Use the PN_CUMULATIVE flag to indicate everything up to
 * (and including) the given tracker.
 *
 * @param[in] messenger the Messenger
 * @param[in] tracker identifies a delivery
 * @param[in] flags 0 or PN_CUMULATIVE
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_settle(pn_messenger_t *messenger, pn_tracker_t tracker, int flags);

/** Returns a tracker for the outgoing message most recently given
 * to pn_messenger_put.  Use this tracker with pn_messenger_status
 * to determine the delivery status of the message, as long as the
 * message is still within your outgoing window.
 *
 * @param[in] messenger the messenger
 *
 * @return a pn_tracker_t or an undefined value if pn_messenger_get
 *         has never been called for the given messenger
 */
PN_EXTERN pn_tracker_t pn_messenger_outgoing_tracker(pn_messenger_t *messenger);

/** Sends or receives any outstanding messages queued for a messenger.
 * This will block for the indicated timeout.
 *
 * @param[in] messenger the Messenger
 * @param[in] timeout the maximum time to block in milliseconds, -1 ==
 * forever, 0 == do not block
 *
 * @return 0 if no work to do, < 0 if error, or 1 if work was done.
 */
PN_EXTERN int pn_messenger_work(pn_messenger_t *messenger, int timeout);

/** The messenger interface is single-threaded.
 * This is the only messenger function intended to be called
 * from outside of the messenger thread.
 * It will interrupt any messenger function which is currently blocking.
 *
 * @param[in] messenger the Messenger
 */
PN_EXTERN int pn_messenger_interrupt(pn_messenger_t *messenger);

/** If blocking has been set with pn_messenger_set_blocking, this call
 * will block until N messages have been sent.  A value of -1 for N means
 * "all messages in the outgoing queue".
 *
 * In addition, if a nonzero size has been set for the outgoing window,
 * this call will block until all messages within that window have
 * been settled, or until all N messages have been settled, whichever
 * comes first.
 *
 * Any blocking will end upon timeout, if one has been set by
 * pn_messenger_timeout.
 *
 * If blocking has not been enabled, this call will stop transmitting
 * messages when further transmission would require blocking, or when
 * the outgoing queue is empty, or when n messages have been sent.
 *
 * @param[in] messenger the messager
 * @param[in] n the number of messages to send
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_send(pn_messenger_t *messenger, int n);

/** Instructs the messenger to receives up to limit messages into the
 * incoming message queue of a messenger. If limit is -1, Messenger
 * will receive as many messages as it can buffer internally. If the
 * messenger is in blocking mode, this call will block until at least
 * one message is available in the incoming queue.
 *
 * Each call to pn_messenger_recv replaces the previous receive
 * operation, so pn_messenger_recv(messenger, 0) will cancel any
 * outstanding receive.
 *
 * @param[in] messenger the messenger
 * @param[in] limit the maximum number of messages to receive or -1 to
 *                  to receive as many messages as it can buffer
 *                  internally.
 *
 * After receiving messages onto your incoming queue
 * use pn_messenger_get to bring messages into your application code.
 * 
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_recv(pn_messenger_t *messenger, int limit);

/** Returns the capacity of the incoming message queue of
 * messenger. Note this count does not include those messages already
 * available on the incoming queue (@see
 * pn_messenger_incoming()). Rather it returns the number of incoming
 * queue entries available for receiving messages
 *
 * @param[in] messenger the messenger
 */
PN_EXTERN int pn_messenger_receiving(pn_messenger_t *messenger);

/** Pop the oldest message off your incoming message queue,
 * and copy it into the given message structure.
 * If the given pointer to a message structure is NULL,
 * the popped message is discarded.
 * Returns PN_EOS if there are no messages to get.
 * Returns an error code only if there is a problem in
 * decoding the message.
 *
 * @param[in] messenger the messenger
 * @param[out] msg upon return contains the message from the head of the queue
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_get(pn_messenger_t *messenger, pn_message_t *msg);

/** Returns a tracker for the message most recently fetched by
 * pn_messenger_get.  The tracker allows you to accept or reject its
 * message, or its message plus all prior messages that are still within
 * your incoming window.
 *
 * @param[in] messenger the messenger
 *
 * @return a pn_tracker_t or an undefined value if pn_messenger_get
 *         has never been called for the given messenger
 */
PN_EXTERN pn_tracker_t pn_messenger_incoming_tracker(pn_messenger_t *messenger);

/** Returns a pointer to the subscription of the message returned by the
 * most recent call to pn_messenger_get, or NULL if pn_messenger_get
 * has not yet been called.
 *
 * @param[in] messenger the messenger
 *
 * @return a pn_subscription_t or NULL if pn_messenger_get
 *         has never been called for the given messenger
 */
PN_EXTERN pn_subscription_t *pn_messenger_incoming_subscription(pn_messenger_t *messenger);

#define PN_CUMULATIVE (0x1)

/** Signal the sender that you have acted on the message
 * pointed to by the tracker.  If the PN_CUMULATIVE flag is set, all
 * messages prior to the tracker will also be accepted, back to the
 * beginning of your incoming window.
 *
 * @param[in] messenger the messenger
 * @param[in] tracker an incoming tracker
 * @param[in] flags 0 or PN_CUMULATIVE
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_accept(pn_messenger_t *messenger, pn_tracker_t tracker, int flags);

/** Rejects the message indicated by the tracker.  If the PN_CUMULATIVE
 * flag is used this call will also reject all prior messages that
 * have not already been settled.  The semantics of message rejection
 * are application-specific.
 *
 * @param[in] messenger the Messenger
 * @param[in] tracker an incoming tracker
 * @param[in] flags 0 or PN_CUMULATIVE
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_reject(pn_messenger_t *messenger, pn_tracker_t tracker, int flags);

/** Returns the number of messages in the outgoing message queue of a messenger.
 *
 * @param[in] messenger the Messenger
 *
 * @return the outgoing queue depth
 */
PN_EXTERN int pn_messenger_outgoing(pn_messenger_t *messenger);

/** Returns the number of messages in the incoming message queue of a messenger.
 *
 * @param[in] messenger the Messenger
 *
 * @return the incoming queue depth
 */
PN_EXTERN int pn_messenger_incoming(pn_messenger_t *messenger);

//! Adds a routing rule to a Messenger's internal routing table.
//!
//! The route procedure may be used to influence how a messenger will
//! internally treat a given address or class of addresses. Every call
//! to the route procedure will result in messenger appending a routing
//! rule to its internal routing table.
//!
//! Whenever a message is presented to a messenger for delivery, it
//! will match the address of this message against the set of routing
//! rules in order. The first rule to match will be triggered, and
//! instead of routing based on the address presented in the message,
//! the messenger will route based on the address supplied in the rule.
//!
//! The pattern matching syntax supports two types of matches, a '%'
//! will match any character except a '/', and a '*' will match any
//! character including a '/'.
//!
//! A routing address is specified as a normal AMQP address, however it
//! may additionally use substitution variables from the pattern match
//! that triggered the rule.
//!
//! Any message sent to "foo" will be routed to "amqp://foo.com":
//!
//!   pn_messenger_route("foo", "amqp://foo.com");
//!
//! Any message sent to "foobar" will be routed to
//! "amqp://foo.com/bar":
//!
//!   pn_messenger_route("foobar", "amqp://foo.com/bar");
//!
//! Any message sent to bar/<path> will be routed to the corresponding
//! path within the amqp://bar.com domain:
//!
//!   pn_messenger_route("bar/*", "amqp://bar.com/$1");
//!
//! Route all messages over TLS:
//!
//!   pn_messenger_route("amqp:*", "amqps:$1")
//!
//! Supply credentials for foo.com:
//!
//!   pn_messenger_route("amqp://foo.com/*", "amqp://user:password@foo.com/$1");
//!
//! Supply credentials for all domains:
//!
//!   pn_messenger_route("amqp://*", "amqp://user:password@$1");
//!
//! Route all addresses through a single proxy while preserving the
//! original destination:
//!
//!   pn_messenger_route("amqp://%/*", "amqp://user:password@proxy/$1/$2");
//!
//! Route any address through a single broker:
//!
//!   pn_messenger_route("*", "amqp://user:password@broker/$1");
//!
//! @param[in] messenger the Messenger
//! @param[in] pattern a glob pattern
//! @param[in] address an address indicating alternative routing
//!
//! @return an error code or zero on success
//! @see error.h
PN_EXTERN int pn_messenger_route(pn_messenger_t *messenger, const char *pattern,
                                 const char *address);

/** Similar to pn_messenger_route, except that the destination of
 * the message is determined before the message address is rewritten.
 *
 * The outgoing address is only rewritten after routing has been
 * finalized.  If a message has an outgoing address of
 * "amqp://0.0.0.0:5678", and a rewriting rule that changes its
 * outgoing address to "foo", it will still arrive at the peer that
 * is listening on "amqp://0.0.0.0:5678", but when it arrives there,
 * the receiver will see its outgoing address as "foo".
 *
 * The default rewrite rule removes username and password from addresses
 * before they are transmitted.
 * 
 * @param[in] messenger the Messenger
 * @param[in] pattern a glob pattern to select messages
 * @param[in] address an address indicating outgoing address rewrite
 *
 */
PN_EXTERN int pn_messenger_rewrite(pn_messenger_t *messenger, const char *pattern,
                                   const char *address);

#ifdef __cplusplus
}
#endif

#endif /* messenger.h */
