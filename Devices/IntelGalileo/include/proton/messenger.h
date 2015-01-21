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
#include <proton/selectable.h>
#include <proton/condition.h>
#include <proton/terminus.h>
#include <proton/link.h>
#include <proton/transport.h>
#include <proton/ssl.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * @file
 *
 * The messenger API provides a high level interface for sending and
 * receiving AMQP messages.
 *
 * @defgroup messenger Messenger
 * @{
 */

/**
 * A ::pn_messenger_t provides a high level interface for sending and
 * receiving messages (See ::pn_message_t).
 *
 * Every messenger contains a single logical queue of incoming
 * messages and a single logical queue of outgoing messages. The
 * messages in these queues may be destined for, or originate from, a
 * variety of addresses.
 *
 * The messenger interface is single-threaded. All methods except one
 * (::pn_messenger_interrupt()) are intended to be used by one thread
 * at a time.
 *
 *
 * Address Syntax
 * ==============
 *
 * An address has the following form::
 *
 *   [ amqp[s]:// ] [user[:password]@] domain [/[name]]
 *
 * Where domain can be one of::
 *
 *   host | host:port | ip | ip:port | name
 *
 * The following are valid examples of addresses:
 *
 *  - example.org
 *  - example.org:1234
 *  - amqp://example.org
 *  - amqps://example.org
 *  - example.org/incoming
 *  - amqps://example.org/outgoing
 *  - amqps://fred:trustno1@example.org
 *  - 127.0.0.1:1234
 *  - amqps://127.0.0.1:1234
 *
 * Sending & Receiving Messages
 * ============================
 *
 * The messenger API works in conjuction with the ::pn_message_t API.
 * A ::pn_message_t is a mutable holder of message content.
 *
 * The ::pn_messenger_put() operation copies content from the supplied
 * ::pn_message_t to the outgoing queue, and may send queued messages
 * if it can do so without blocking. The ::pn_messenger_send()
 * operation blocks until it has sent the requested number of
 * messages, or until a timeout interrupts the attempt.
 *
 *
 *   pn_messenger_t *messenger = pn_messenger(NULL);
 *   pn_message_t *message = pn_message();
 *   char subject[1024];
 *   for (int i = 0; i < 3; i++) {
 *     pn_message_set_address(message, "amqp://host/queue");
 *     sprintf(subject, "Hello World! %i", i);
 *     pn_message_set_subject(message, subject);
 *     pn_messenger_put(messenger, message)
 *   pn_messenger_send(messenger);
 *
 * Similarly, the ::pn_messenger_recv() method receives messages into
 * the incoming queue, and may block as it attempts to receive up to
 * the requested number of messages, or until the timeout is reached.
 * It may receive fewer than the requested number. The
 * ::pn_messenger_get() method pops the eldest message off the
 * incoming queue and copies its content into the supplied
 * ::pn_message_t object. It will not block.
 *
 *
 *   pn_messenger_t *messenger = pn_messenger(NULL);
 *   pn_message_t *message = pn_message()
 *   pn_messenger_recv(messenger):
 *   while (pn_messenger_incoming(messenger) > 0) {
 *     pn_messenger_get(messenger, message);
 *     printf("%s", message.subject);
 *   }
 *
 *   Output:
 *     Hello World 0
 *     Hello World 1
 *     Hello World 2
 *
 * The blocking flag allows you to turn off blocking behavior
 * entirely, in which case ::pn_messenger_send() and
 * ::pn_messenger_recv() will do whatever they can without blocking,
 * and then return. You can then look at the number of incoming and
 * outgoing messages to see how much outstanding work still remains.
 */
typedef struct pn_messenger_t pn_messenger_t;

/**
 * A subscription is a request for incoming messages.
 *
 * @todo currently the subscription API is under developed, this
 * should allow more explicit control over subscription properties and
 * behaviour
 */
typedef struct pn_subscription_t pn_subscription_t;

/**
 * Trackers provide a lightweight handle used to track the status of
 * incoming and outgoing deliveries.
 */
typedef int64_t pn_tracker_t;

/**
 * Describes all the possible states for a message associated with a
 * given tracker.
 */
typedef enum {
  PN_STATUS_UNKNOWN = 0, /**< The tracker is unknown. */
  PN_STATUS_PENDING = 1, /**< The message is in flight. For outgoing
                            messages, use ::pn_messenger_buffered to
                            see if it has been sent or not. */
  PN_STATUS_ACCEPTED = 2, /**< The message was accepted. */
  PN_STATUS_REJECTED = 3, /**< The message was rejected. */
  PN_STATUS_RELEASED = 4, /**< The message was released. */
  PN_STATUS_MODIFIED = 5, /**< The message was modified. */
  PN_STATUS_ABORTED = 6, /**< The message was aborted. */
  PN_STATUS_SETTLED = 7 /**< The remote party has settled the message. */
} pn_status_t;

/**
 * Construct a new ::pn_messenger_t with the given name. The name is
 * global. If a NULL name is supplied, a UUID based name will be
 * chosen.
 *
 * @param[in] name the name of the messenger or NULL
 *
 * @return pointer to a new ::pn_messenger_t
 */
PN_EXTERN pn_messenger_t *pn_messenger(const char *name);

/**
 * Get the name of a messenger.
 *
 * @param[in] messenger a messenger object
 * @return the name of the messenger
 */
PN_EXTERN const char *pn_messenger_name(pn_messenger_t *messenger);

/**
 * Sets the path that will be used to get the certificate that will be
 * used to identify this messenger to its peers. The validity of the
 * path is not checked by this function.
 *
 * @param[in] messenger the messenger
 * @param[in] certificate a path to a certificate file
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_certificate(pn_messenger_t *messenger, const char *certificate);

/**
 * Get the certificate path. This value may be set by
 * pn_messenger_set_certificate. The default certificate path is null.
 *
 * @param[in] messenger the messenger
 * @return the certificate file path
 */
PN_EXTERN const char *pn_messenger_get_certificate(pn_messenger_t *messenger);

/**
 * Set path to the private key that was used to sign the certificate.
 * See ::pn_messenger_set_certificate
 *
 * @param[in] messenger a messenger object
 * @param[in] private_key a path to a private key file
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_private_key(pn_messenger_t *messenger, const char *private_key);

/**
 * Gets the private key file for a messenger.
 *
 * @param[in] messenger a messenger object
 * @return the messenger's private key file path
 */
PN_EXTERN const char *pn_messenger_get_private_key(pn_messenger_t *messenger);

/**
 * Sets the private key password for a messenger.
 *
 * @param[in] messenger a messenger object
 * @param[in] password the password for the private key file
 *
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_password(pn_messenger_t *messenger, const char *password);

/**
 * Gets the private key file password for a messenger.
 *
 * @param[in] messenger a messenger object
 * @return password for the private key file
 */
PN_EXTERN const char *pn_messenger_get_password(pn_messenger_t *messenger);

/**
 * Sets the trusted certificates database for a messenger.
 *
 * The messenger will use this database to validate the certificate
 * provided by the peer.
 *
 * @param[in] messenger a messenger object
 * @param[in] cert_db a path to the certificates database
 *
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_trusted_certificates(pn_messenger_t *messenger, const char *cert_db);

/**
 * Gets the trusted certificates database for a messenger.
 *
 * @param[in] messenger a messenger object
 * @return path to the trusted certificates database
 */
PN_EXTERN const char *pn_messenger_get_trusted_certificates(pn_messenger_t *messenger);

/**
 * Set the default timeout for a messenger.
 *
 * Any messenger call that blocks during execution will stop blocking
 * and return control when this timeout is reached, if you have set it
 * to a value greater than zero. The timeout is expressed in
 * milliseconds.
 *
 * @param[in] messenger a messenger object
 * @param[in] timeout a new timeout for the messenger, in milliseconds
 * @return an error code or zero if there is no error
 */
PN_EXTERN int pn_messenger_set_timeout(pn_messenger_t *messenger, int timeout);

/**
 * Gets the timeout for a messenger object.
 *
 * See ::pn_messenger_set_timeout() for details.
 *
 * @param[in] messenger a messenger object
 * @return the timeout for the messenger, in milliseconds
 */
PN_EXTERN int pn_messenger_get_timeout(pn_messenger_t *messenger);

/**
 * Check if a messenger is in blocking mode.
 *
 * @param[in] messenger a messenger object
 * @return true if blocking has been enabled, false otherwise
 */
PN_EXTERN bool pn_messenger_is_blocking(pn_messenger_t *messenger);

/**
 * Enable or disable blocking behavior for a messenger during calls to
 * ::pn_messenger_send and ::pn_messenger_recv.
 *
 * @param[in] messenger a messenger object
 * @param[in] blocking the value of the blocking flag
 * @return an error code or zero if there is no error
 */
PN_EXTERN int pn_messenger_set_blocking(pn_messenger_t *messenger, bool blocking);

/**
 * Check if a messenger is in passive mode.
 *
 * A messenger that is in passive mode will never attempt to perform
 * I/O internally, but instead will make all internal file descriptors
 * accessible through ::pn_messenger_selectable() to be serviced
 * externally. This can be useful for integrating messenger into an
 * external event loop.
 *
 * @param[in] messenger a messenger object
 * @return true if the messenger is in passive mode, false otherwise
 */
PN_EXTERN bool pn_messenger_is_passive(pn_messenger_t *messenger);

/**
 * Set the passive mode for a messenger.
 *
 * See ::pn_messenger_is_passive() for details on passive mode.
 *
 * @param[in] messenger a messenger object
 * @param[in] passive true to enable passive mode, false to disable
 * passive mode
 * @return an error code or zero on success
 */
PN_EXTERN int pn_messenger_set_passive(pn_messenger_t *messenger, bool passive);

/** Frees a Messenger.
 *
 * @param[in] messenger the messenger to free (or NULL), no longer
 *                      valid on return
 */
PN_EXTERN void pn_messenger_free(pn_messenger_t *messenger);

/**
 * Get the code for a messenger's most recent error.
 *
 * The error code is initialized to zero at messenger creation. The
 * error number is "sticky" i.e. error codes are not reset to 0 at the
 * end of successful API calls. You can use ::pn_messenger_error to
 * access the messenger's error object and clear explicitly if
 * desired.
 *
 * @param[in] messenger the messenger to check for errors
 * @return an error code or zero if there is no error
 * @see error.h
 */
PN_EXTERN int pn_messenger_errno(pn_messenger_t *messenger);

/**
 * Get a messenger's error object.
 *
 * Returns a pointer to a pn_error_t that is valid until the messenger
 * is freed. The pn_error_* API allows you to access the text, error
 * number, and lets you set or clear the error code explicitly.
 *
 * @param[in] messenger the messenger to check for errors
 * @return a pointer to the messenger's error descriptor
 * @see error.h
 */
PN_EXTERN pn_error_t *pn_messenger_error(pn_messenger_t *messenger);

/**
 * Get the size of a messenger's outgoing window.
 *
 * The size of the outgoing window limits the number of messages whose
 * status you can check with a tracker. A message enters this window
 * when you call pn_messenger_put on the message. For example, if your
 * outgoing window size is 10, and you call pn_messenger_put 12 times,
 * new status information will no longer be available for the first 2
 * messages.
 *
 * The default outgoing window size is 0.
 *
 * @param[in] messenger a messenger object
 * @return the outgoing window for the messenger
 */
PN_EXTERN int pn_messenger_get_outgoing_window(pn_messenger_t *messenger);

/**
 * Set the size of a messenger's outgoing window.
 *
 * See ::pn_messenger_get_outgoing_window() for details.
 *
 * @param[in] messenger a messenger object
 * @param[in] window the number of deliveries to track
 * @return an error or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_set_outgoing_window(pn_messenger_t *messenger, int window);

/**
 * Get the size of a messenger's incoming window.
 *
 * The size of a messenger's incoming window limits the number of
 * messages that can be accepted or rejected using trackers. Messages
 * *do not* enter this window when they have been received
 * (::pn_messenger_recv) onto you incoming queue. Messages only enter
 * this window only when you access them using pn_messenger_get. If
 * your incoming window size is N, and you get N+1 messages without
 * explicitly accepting or rejecting the oldest message, then it will
 * be implicitly accepted when it falls off the edge of the incoming
 * window.
 *
 * The default incoming window size is 0.
 *
 * @param[in] messenger a messenger object
 * @return the incoming window for the messenger
 */
PN_EXTERN int pn_messenger_get_incoming_window(pn_messenger_t *messenger);

/**
 * Set the size of a messenger's incoming window.
 *
 * See ::pn_messenger_get_incoming_window() for details.
 *
 * @param[in] messenger a messenger object
 * @param[in] window the number of deliveries to track
 * @return an error or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_set_incoming_window(pn_messenger_t *messenger,
                                               int window);

/**
 * Currently a no-op placeholder. For future compatibility, do not
 * send or receive messages before starting the messenger.
 *
 * @param[in] messenger the messenger to start
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_start(pn_messenger_t *messenger);

/**
 * Stops a messenger.
 *
 * Stopping a messenger will perform an orderly shutdown of all
 * underlying connections. This may require some time. If the
 * messenger is in non blocking mode (see ::pn_messenger_is_blocking),
 * this operation will return PN_INPROGRESS if it cannot finish
 * immediately. In that case, you can use ::pn_messenger_stopped() to
 * determine when the messenger has finished stopping.
 *
 * @param[in] messenger the messenger to stop
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_stop(pn_messenger_t *messenger);

/**
 * Returns true if a messenger is in the stopped state. This function
 * does not block.
 *
 * @param[in] messenger the messenger to stop
 *
 */
PN_EXTERN bool pn_messenger_stopped(pn_messenger_t *messenger);

/**
 * Subscribes a messenger to messages from the specified source.
 *
 * @param[in] messenger the messenger to subscribe
 * @param[in] source
 * @return a subscription
 */
PN_EXTERN pn_subscription_t *pn_messenger_subscribe(pn_messenger_t *messenger, const char *source);

/**
 * Subscribes a messenger to messages from the specified source with the given
 * timeout for the subscription's lifetime.
 *
 * @param[in] messenger the messenger to subscribe
 * @param[in] source
 * @param[in] timeout the maximum time to keep the subscription alive once the
 *            link is closed.
 * @return a subscription
 */
PN_EXTERN pn_subscription_t *
pn_messenger_subscribe_ttl(pn_messenger_t *messenger, const char *source,
                           pn_seconds_t timeout);

/**
 * Get a link based on link name and whether the link is a sender or receiver
 *
 * @param[in] messenger the messenger to get the link from
 * @param[in] address the link address that identifies the link to receive
 * @param[in] sender true if the link is a sender, false if the link is a
 *            receiver
 * @return a link, or NULL if no link matches the address / sender parameters
 */
PN_EXTERN pn_link_t *pn_messenger_get_link(pn_messenger_t *messenger,
                                           const char *address, bool sender);

/**
 * Get a subscription's application context.
 *
 * See ::pn_subscription_set_context().
 *
 * @param[in] sub a subscription object
 * @return the subscription's application context
 */
PN_EXTERN void *pn_subscription_get_context(pn_subscription_t *sub);

/**
 * Set an application context for a subscription.
 *
 * @param[in] sub a subscription object
 * @param[in] context the application context for the subscription
 */
PN_EXTERN void pn_subscription_set_context(pn_subscription_t *sub, void *context);

/**
 * Get the source address of a subscription.
 *
 * @param[in] sub a subscription object
 * @return the subscription's source address
 */
PN_EXTERN const char *pn_subscription_address(pn_subscription_t *sub);

/**
 * Puts a message onto the messenger's outgoing queue. The message may
 * also be sent if transmission would not cause blocking. This call
 * will not block.
 *
 * @param[in] messenger a messenger object
 * @param[in] msg a message to put on the messenger's outgoing queue
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_put(pn_messenger_t *messenger, pn_message_t *msg);

/**
 * Track the status of a delivery.
 *
 * Get the current status of the delivery associated with the supplied
 * tracker. This may return PN_STATUS_UNKOWN if the tracker has fallen
 * outside the incoming/outgoing tracking windows of the messenger.
 *
 * @param[in] messenger the messenger
 * @param[in] tracker the tracker identifying the delivery
 * @return a status code for the delivery
 */
PN_EXTERN pn_status_t pn_messenger_status(pn_messenger_t *messenger, pn_tracker_t tracker);

/**
 * Get delivery information about a delivery.
 *
 * Returns the delivery information associated with the supplied tracker.
 * This may return NULL if the tracker has fallen outside the
 * incoming/outgoing tracking windows of the messenger.
 *
 * @param[in] messenger the messenger
 * @param[in] tracker the tracker identifying the delivery
 * @return a pn_delivery_t representing the delivery.
 */
PN_EXTERN pn_delivery_t *pn_messenger_delivery(pn_messenger_t *messenger,
                                               pn_tracker_t tracker);

/**
 * Check if the delivery associated with a given tracker is still
 * waiting to be sent.
 *
 * Note that returning false does not imply that the delivery was
 * actually sent over the wire.
 *
 * @param[in] messenger the messenger
 * @param[in] tracker the tracker identifying the delivery
 *
 * @return true if the delivery is still buffered
 */
PN_EXTERN bool pn_messenger_buffered(pn_messenger_t *messenger, pn_tracker_t tracker);

/**
 * Frees a Messenger from tracking the status associated with a given
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

/**
 * Get a tracker for the outgoing message most recently given to
 * pn_messenger_put.
 *
 * This tracker may be used with pn_messenger_status to determine the
 * delivery status of the message, as long as the message is still
 * within your outgoing window.
 *
 * @param[in] messenger the messenger
 *
 * @return a pn_tracker_t or an undefined value if pn_messenger_get
 *         has never been called for the given messenger
 */
PN_EXTERN pn_tracker_t pn_messenger_outgoing_tracker(pn_messenger_t *messenger);

/**
 * Sends or receives any outstanding messages queued for a messenger.
 * This will block for the indicated timeout.
 *
 * @param[in] messenger the Messenger
 * @param[in] timeout the maximum time to block in milliseconds, -1 ==
 * forever, 0 == do not block
 *
 * @return 0 if no work to do, < 0 if error, or 1 if work was done.
 */
PN_EXTERN int pn_messenger_work(pn_messenger_t *messenger, int timeout);

/**
 * Interrupt a messenger object that may be blocking in another
 * thread.
 *
 * The messenger interface is single-threaded. This is the only
 * messenger function intended to be concurrently called from another
 * thread. It will interrupt any messenger function which is currently
 * blocking and cause it to return with a status of ::PN_INTR.
 *
 * @param[in] messenger the Messenger to interrupt
 */
PN_EXTERN int pn_messenger_interrupt(pn_messenger_t *messenger);

/**
 * Send messages from a messenger's outgoing queue.
 *
 * If a messenger is in blocking mode (see
 * ::pn_messenger_is_blocking()), this operation will block until N
 * messages have been sent from the outgoing queue. A value of -1 for
 * N means "all messages in the outgoing queue". See below for a full
 * definition of what sent from the outgoing queue means.
 *
 * Any blocking will end once the messenger's configured timeout (if
 * any) has been reached. When this happens an error code of
 * ::PN_TIMEOUT is returned.
 *
 * If the messenger is in non blocking mode, this call will return an
 * error code of ::PN_INPROGRESS if it is unable to send the requested
 * number of messages without blocking.
 *
 * A message is considered to be sent from the outgoing queue when its
 * status has been fully determined. This does not necessarily mean
 * the message was successfully sent to the final recipient though,
 * for example of the receiver rejects the message, the final status
 * will be ::PN_STATUS_REJECTED. Similarly, if a message is sent to an
 * invalid address, it may be removed from the outgoing queue without
 * ever even being transmitted. In this case the final status will be
 * ::PN_STATUS_ABORTED.
 *
 * @param[in] messenger a messenger object
 * @param[in] n the number of messages to send
 *
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_send(pn_messenger_t *messenger, int n);

/**
 * Retrieve messages into a messenger's incoming queue.
 *
 * Instructs a messenger to receive up to @c limit messages into the
 * incoming message queue of a messenger. If @c limit is -1, the
 * messenger will receive as many messages as it can buffer
 * internally. If the messenger is in blocking mode, this call will
 * block until at least one message is available in the incoming
 * queue.
 *
 * Each call to pn_messenger_recv replaces the previous receive
 * operation, so pn_messenger_recv(messenger, 0) will cancel any
 * outstanding receive.
 *
 * After receiving messages onto your incoming queue use
 * ::pn_messenger_get() to access message content.
 *
 * @param[in] messenger the messenger
 * @param[in] limit the maximum number of messages to receive or -1 to
 *                  to receive as many messages as it can buffer
 *                  internally.
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_recv(pn_messenger_t *messenger, int limit);

/**
 * Get the capacity of the incoming message queue of a messenger.
 *
 * Note this count does not include those messages already available
 * on the incoming queue (@see pn_messenger_incoming()). Rather it
 * returns the number of incoming queue entries available for
 * receiving messages.
 *
 * @param[in] messenger the messenger
 */
PN_EXTERN int pn_messenger_receiving(pn_messenger_t *messenger);

/**
 * Get the next message from the head of a messenger's incoming queue.
 *
 * The get operation copies the message data from the head of the
 * messenger's incoming queue into the provided ::pn_message_t object.
 * If provided ::pn_message_t pointer is NULL, the head essage will be
 * discarded. This operation will return ::PN_EOS if there are no
 * messages left on the incoming queue.
 *
 * @param[in] messenger a messenger object
 * @param[out] message upon return contains the message from the head of the queue
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_get(pn_messenger_t *messenger, pn_message_t *message);

/**
 * Get a tracker for the message most recently retrieved by
 * ::pn_messenger_get().
 *
 * A tracker for an incoming message allows you to accept or reject
 * the associated message. It can also be used for cumulative
 * accept/reject operations for the associated message and all prior
 * messages as well.
 *
 * @param[in] messenger a messenger object
 * @return a pn_tracker_t or an undefined value if pn_messenger_get
 *         has never been called for the given messenger
 */
PN_EXTERN pn_tracker_t pn_messenger_incoming_tracker(pn_messenger_t *messenger);

/**
 * Get the subscription of the message most recently retrieved by ::pn_messenger_get().
 *
 * This operation will return NULL if ::pn_messenger_get() has never
 * been succesfully called.
 *
 * @param[in] messenger a messenger object
 * @return a pn_subscription_t or NULL
 */
PN_EXTERN pn_subscription_t *pn_messenger_incoming_subscription(pn_messenger_t *messenger);

/**
 * Indicates that an accept or reject should operate cumulatively.
 */
#define PN_CUMULATIVE (0x1)

/**
 * Signal successful processing of message(s).
 *
 * With no flags this operation will signal the sender that the
 * message referenced by the tracker was accepted. If the
 * PN_CUMULATIVE flag is set, this operation will also reject all
 * pending messages prior to the message indicated by the tracker.
 *
 * Note that when a message is accepted or rejected multiple times,
 * either explicitly, or implicitly through use of the ::PN_CUMULATIVE
 * flag, only the first outcome applies. For example if a sequence of
 * three messages are received: M1, M2, M3, and M2 is rejected, and M3
 * is cumulatively accepted, M2 will remain rejected and only M1 and
 * M3 will be considered accepted.
 *
 * @param[in] messenger a messenger object
 * @param[in] tracker an incoming tracker
 * @param[in] flags 0 or PN_CUMULATIVE
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_accept(pn_messenger_t *messenger, pn_tracker_t tracker, int flags);

/**
 * Signal unsuccessful processing of message(s).
 *
 * With no flags this operation will signal the sender that the
 * message indicated by the tracker was rejected. If the PN_CUMULATIVE
 * flag is used this operation will also reject all pending messages
 * prior to the message indicated by the tracker.
 *
 * Note that when a message is accepted or rejected multiple times,
 * either explicitly, or implicitly through use of the ::PN_CUMULATIVE
 * flag, only the first outcome applies. For example if a sequence of
 * three messages are received: M1, M2, M3, and M2 is accepted, and M3
 * is cumulatively rejected, M2 will remain accepted and only M1 and
 * M3 will be considered rejected.
 *
 * @param[in] messenger a messenger object
 * @param[in] tracker an incoming tracker
 * @param[in] flags 0 or PN_CUMULATIVE
 * @return an error code or zero on success
 * @see error.h
 */
PN_EXTERN int pn_messenger_reject(pn_messenger_t *messenger, pn_tracker_t tracker, int flags);

/**
 * Get  link for the message referenced by the given tracker.
 *
 * @param[in] messenger a messenger object
 * @param[in] tracker a tracker object
 * @return a pn_link_t or NULL if the link could not be determined.
 */
PN_EXTERN pn_link_t *pn_messenger_tracker_link(pn_messenger_t *messenger,
                                               pn_tracker_t tracker);

/**
 * Get the number of messages in the outgoing message queue of a
 * messenger.
 *
 * @param[in] messenger a messenger object
 * @return the outgoing queue depth
 */
PN_EXTERN int pn_messenger_outgoing(pn_messenger_t *messenger);

/**
 * Get the number of messages in the incoming message queue of a messenger.
 *
 * @param[in] messenger a messenger object
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
//! Any message sent to bar/&lt;path&gt; will be routed to the corresponding
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

/**
 * Rewrite message addresses prior to transmission.
 *
 * This operation is similar to pn_messenger_route, except that the
 * destination of the message is determined before the message address
 * is rewritten.
 *
 * The outgoing address is only rewritten after routing has been
 * finalized.  If a message has an outgoing address of
 * "amqp://0.0.0.0:5678", and a rewriting rule that changes its
 * outgoing address to "foo", it will still arrive at the peer that
 * is listening on "amqp://0.0.0.0:5678", but when it arrives there,
 * the receiver will see its outgoing address as "foo".
 *
 * The default rewrite rule removes username and password from
 * addresses before they are transmitted.
 *
 * @param[in] messenger a messenger object
 * @param[in] pattern a glob pattern to select messages
 * @param[in] address an address indicating outgoing address rewrite
 * @return an error code or zero on success
 */
PN_EXTERN int pn_messenger_rewrite(pn_messenger_t *messenger, const char *pattern,
                                   const char *address);

/**
 * Extract @link pn_selectable_t selectables @endlink from a passive
 * messenger.
 *
 * A messenger that is in passive mode (see
 * ::pn_messenger_is_passive()) will never attempt to perform any I/O
 * internally, but instead make its internal file descriptors
 * available for external processing via the
 * ::pn_messenger_selectable() operation.
 *
 * An application wishing to perform I/O on behalf of a passive
 * messenger must extract all available selectables by calling this
 * operation until it returns NULL. The ::pn_selectable_t interface
 * may then be used by the application to perform I/O outside the
 * messenger.
 *
 * All selectables returned by this operation must be serviced until
 * they reach a terminal state and then freed. See
 * ::pn_selectable_is_terminal() for more details.
 *
 * By default any given selectable will only ever be returned once by
 * this operation, however if the selectable's registered flag is set
 * to true (see ::pn_selectable_set_registered()), then the selectable
 * will be returned whenever its interest set may have changed.
 *
 * @param[in] messenger a messenger object
 * @return the next selectable, or NULL if there are none left
 */
PN_EXTERN pn_selectable_t *pn_messenger_selectable(pn_messenger_t *messenger);

/**
 * Get the nearest deadline for selectables associated with a messenger.
 *
 * @param[in] messenger a messenger object
 * @return the nearest deadline
 */
PN_EXTERN pn_timestamp_t pn_messenger_deadline(pn_messenger_t *messenger);

/**
 * @}
 */

#define PN_FLAGS_CHECK_ROUTES                                                  \
  (0x1) /** Messenger flag to indicate that a call                             \
            to pn_messenger_start should check that                            \
            any defined routes are valid */

/** Sets control flags to enable additional function for the Messenger.
 *
 * @param[in] messenger the messenger
 * @param[in] flags 0 or PN_FLAGS_CHECK_ROUTES
 *
 * @return an error code of zero if there is no error
 */
PN_EXTERN int pn_messenger_set_flags(pn_messenger_t *messenger,
                                     const int flags);

/** Gets the flags for a Messenger.
 *
 * @param[in] messenger the messenger
 * @return The flags set for the messenger
 */
PN_EXTERN int pn_messenger_get_flags(pn_messenger_t *messenger);

/**
 * Set the local sender settle mode for the underlying link.
 *
 * @param[in] messenger the messenger
 * @param[in] mode the sender settle mode
 */
PN_EXTERN int pn_messenger_set_snd_settle_mode(pn_messenger_t *messenger,
                                               const pn_snd_settle_mode_t mode);

/**
 * Set the local receiver settle mode for the underlying link.
 *
 * @param[in] messenger the messenger
 * @param[in] mode the receiver settle mode
 */
PN_EXTERN int pn_messenger_set_rcv_settle_mode(pn_messenger_t *messenger,
                                               const pn_rcv_settle_mode_t mode);

/**
 * Set the tracer associated with a messenger.
 *
 * @param[in] messenger a messenger object
 * @param[in] tracer the tracer callback
 */
PN_EXTERN void pn_messenger_set_tracer(pn_messenger_t *messenger,
                                       pn_tracer_t tracer);

/**
 * Gets the remote idle timeout for the specified remote service address
 *
 * @param[in] messenger a messenger object
 * @param[in] address of remote service whose idle timeout is required
 * @return the timeout in milliseconds or -1 if an error occurs
 */
PN_EXTERN pn_millis_t
    pn_messenger_get_remote_idle_timeout(pn_messenger_t *messenger,
                                         const char *address);

/**
 * Sets the SSL peer authentiacation mode required when a trust
 * certificate is used.
 *
 * @param[in] messenger a messenger object
 * @param[in] mode the mode required (see pn_ssl_verify_mode_t
 *             enum for valid values)
 * @return 0 if successful or -1 if an error occurs
 */
PN_EXTERN int
pn_messenger_set_ssl_peer_authentication_mode(pn_messenger_t *messenger,
                                              const pn_ssl_verify_mode_t mode);

#ifdef __cplusplus
}
#endif

#endif /* messenger.h */
