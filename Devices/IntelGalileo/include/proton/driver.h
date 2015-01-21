#ifndef PROTON_DRIVER_H
#define PROTON_DRIVER_H 1

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
#include <proton/error.h>
#include <proton/sasl.h>
#include <proton/selectable.h>
#include <proton/ssl.h>
#include <proton/transport.h>
#include <proton/types.h>

#ifdef __cplusplus
extern "C" {
#endif

/** @file
 * API for the Driver Layer.
 *
 * The driver library provides a simple implementation of a driver for
 * the proton engine. A driver is responsible for providing input,
 * output, and tick events to the bottom half of the engine API. See
 * ::pn_transport_input, ::pn_transport_output, and
 * ::pn_transport_tick. The driver also provides an interface for the
 * application to access the top half of the API when the state of the
 * engine may have changed due to I/O or timing events. Additionally
 * the driver incorporates the SASL engine as well in order to provide
 * a complete network stack: AMQP over SASL over TCP.
 *
 */

typedef struct pn_driver_t pn_driver_t;
typedef struct pn_listener_t pn_listener_t;
typedef struct pn_connector_t pn_connector_t;

typedef enum {
  PN_CONNECTOR_WRITABLE,
  PN_CONNECTOR_READABLE
} pn_activate_criteria_t;

/** Construct a driver
 *
 *  Call pn_driver_free() to release the driver object.
 *  @return new driver object, NULL if error
 */
PN_EXTERN pn_driver_t *pn_driver(void);

/** Return the most recent error code.
 *
 * @param[in] d the driver
 *
 * @return the most recent error text for d
 */
PN_EXTERN int pn_driver_errno(pn_driver_t *d);

/** Get additional error information associated with the driver.
 *
 * Whenever a driver operation fails, additional error information can
 * be obtained using this function. The error object that is returned
 * may also be used to clear the error condition.
 *
 * The pointer returned by this operation is valid until the
 * driver object is freed.
 *
 * @param[in] d the driver
 *
 * @return the driver's error object
 */
PN_EXTERN pn_error_t *pn_driver_error(pn_driver_t *d);

/** Set the tracing level for the given driver.
 *
 * @param[in] driver the driver to trace
 * @param[in] trace the trace level to use.
 * @todo pn_trace_t needs documentation
 */
PN_EXTERN void pn_driver_trace(pn_driver_t *driver, pn_trace_t trace);

/** Force pn_driver_wait() to return
 *
 * @param[in] driver the driver to wake up
 *
 * @return zero on success, an error code on failure
 */
PN_EXTERN int pn_driver_wakeup(pn_driver_t *driver);

/** Wait for an active connector or listener
 *
 * @param[in] driver the driver to wait on
 * @param[in] timeout maximum time in milliseconds to wait, -1 means
 *                    infinite wait
 *
 * @return zero on success, an error code on failure
 */
PN_EXTERN int pn_driver_wait(pn_driver_t *driver, int timeout);

/** Get the next listener with pending data in the driver.
 *
 * @param[in] driver the driver
 * @return NULL if no active listener available
 */
PN_EXTERN pn_listener_t *pn_driver_listener(pn_driver_t *driver);

/** Get the next active connector in the driver.
 *
 * Returns the next connector with pending inbound data, available
 * capacity for outbound data, or pending tick.
 *
 * @param[in] driver the driver
 * @return NULL if no active connector available
 */
PN_EXTERN pn_connector_t *pn_driver_connector(pn_driver_t *driver);

/** Free the driver allocated via pn_driver, and all associated
 *  listeners and connectors.
 *
 * @param[in] driver the driver to free, no longer valid on
 *                   return
 */
PN_EXTERN void pn_driver_free(pn_driver_t *driver);


/** pn_listener - the server API **/

/** Construct a listener for the given address.
 *
 * @param[in] driver driver that will 'own' this listener
 * @param[in] host local host address to listen on
 * @param[in] port local port to listen on
 * @param[in] context application-supplied, can be accessed via
 *                    pn_listener_context()
 * @return a new listener on the given host:port, NULL if error
 */
PN_EXTERN pn_listener_t *pn_listener(pn_driver_t *driver, const char *host,
                           const char *port, void* context);

/** Access the head listener for a driver.
 *
 * @param[in] driver the driver whose head listener will be returned
 *
 * @return the head listener for driver or NULL if there is none
 */
PN_EXTERN pn_listener_t *pn_listener_head(pn_driver_t *driver);

/** Access the next listener.
 *
 * @param[in] listener the listener whose next listener will be
 *            returned
 *
 * @return the next listener
 */
PN_EXTERN pn_listener_t *pn_listener_next(pn_listener_t *listener);

/**
 * @todo pn_listener_trace needs documentation
 */
PN_EXTERN void pn_listener_trace(pn_listener_t *listener, pn_trace_t trace);

/** Accept a connection that is pending on the listener.
 *
 * @param[in] listener the listener to accept the connection on
 * @return a new connector for the remote, or NULL on error
 */
PN_EXTERN pn_connector_t *pn_listener_accept(pn_listener_t *listener);

/** Access the application context that is associated with the listener.
 *
 * @param[in] listener the listener whose context is to be returned
 * @return the application context that was passed to pn_listener() or
 *         pn_listener_fd()
 */
PN_EXTERN void *pn_listener_context(pn_listener_t *listener);

PN_EXTERN void pn_listener_set_context(pn_listener_t *listener, void *context);

/** Close the socket used by the listener.
 *
 * @param[in] listener the listener whose socket will be closed.
 */
PN_EXTERN void pn_listener_close(pn_listener_t *listener);

/** Frees the given listener.
 *
 * Assumes the listener's socket has been closed prior to call.
 *
 * @param[in] listener the listener object to free, no longer valid
 *            on return
 */
PN_EXTERN void pn_listener_free(pn_listener_t *listener);




/** pn_connector - the client API **/

/** Construct a connector to the given remote address.
 *
 * @param[in] driver owner of this connection.
 * @param[in] host remote host to connect to.
 * @param[in] port remote port to connect to.
 * @param[in] context application supplied, can be accessed via
 *                    pn_connector_context() @return a new connector
 *                    to the given remote, or NULL on error.
 */
PN_EXTERN pn_connector_t *pn_connector(pn_driver_t *driver, const char *host,
                             const char *port, void* context);

/** Access the head connector for a driver.
 *
 * @param[in] driver the driver whose head connector will be returned
 *
 * @return the head connector for driver or NULL if there is none
 */
PN_EXTERN pn_connector_t *pn_connector_head(pn_driver_t *driver);

/** Access the next connector.
 *
 * @param[in] connector the connector whose next connector will be
 *            returned
 *
 * @return the next connector
 */
PN_EXTERN pn_connector_t *pn_connector_next(pn_connector_t *connector);

/** Set the tracing level for the given connector.
 *
 * @param[in] connector the connector to trace
 * @param[in] trace the trace level to use.
 */
PN_EXTERN void pn_connector_trace(pn_connector_t *connector, pn_trace_t trace);

/** Service the given connector.
 *
 * Handle any inbound data, outbound data, or timing events pending on
 * the connector.
 *
 * @param[in] connector the connector to process.
 */
PN_EXTERN void pn_connector_process(pn_connector_t *connector);

/** Access the listener which opened this connector.
 *
 * @param[in] connector connector whose listener will be returned.
 * @return the listener which created this connector, or NULL if the
 *         connector has no listener (e.g. an outbound client
 *         connection)
 */
PN_EXTERN pn_listener_t *pn_connector_listener(pn_connector_t *connector);

/** Access the Authentication and Security context of the connector.
 *
 * @param[in] connector connector whose security context will be
 *                      returned
 * @return the Authentication and Security context for the connector,
 *         or NULL if none
 */
PN_EXTERN pn_sasl_t *pn_connector_sasl(pn_connector_t *connector);

/** Access the AMQP Connection associated with the connector.
 *
 * @param[in] connector the connector whose connection will be
 *                      returned
 * @return the connection context for the connector, or NULL if none
 */
PN_EXTERN pn_connection_t *pn_connector_connection(pn_connector_t *connector);

/** Assign the AMQP Connection associated with the connector.
 *
 * @param[in] connector the connector whose connection will be set.
 * @param[in] connection the connection to associate with the
 *                       connector
 */
PN_EXTERN void pn_connector_set_connection(pn_connector_t *connector, pn_connection_t *connection);

/** Access the application context that is associated with the
 *  connector.
 *
 * @param[in] connector the connector whose context is to be returned.
 * @return the application context that was passed to pn_connector()
 *         or pn_connector_fd()
 */
PN_EXTERN void *pn_connector_context(pn_connector_t *connector);

/** Assign a new application context to the connector.
 *
 * @param[in] connector the connector which will hold the context.
 * @param[in] context new application context to associate with the
 *                    connector
 */
PN_EXTERN void pn_connector_set_context(pn_connector_t *connector, void *context);

/** Access the name of the connector
 *
 * @param[in] connector the connector which will hole the name
 * @return the name of the connector in the form of a null-terminated character string.
 */
PN_EXTERN const char *pn_connector_name(const pn_connector_t *connector);

/** Access the transport used by this connector.
 *
 * @param[in] connector connector whose transport will be returned
 * @return the transport, or NULL if none
 */
PN_EXTERN pn_transport_t *pn_connector_transport(pn_connector_t *connector);

/** Close the socket used by the connector.
 *
 * @param[in] connector the connector whose socket will be closed
 */
PN_EXTERN void pn_connector_close(pn_connector_t *connector);

/** Determine if the connector is closed.
 *
 * @return True if closed, otherwise false
 */
PN_EXTERN bool pn_connector_closed(pn_connector_t *connector);

/** Destructor for the given connector.
 *
 * Assumes the connector's socket has been closed prior to call.
 *
 * @param[in] connector the connector object to free. No longer
 *                      valid on return
 */
PN_EXTERN void pn_connector_free(pn_connector_t *connector);

/** Activate a connector when a criteria is met
 *
 * Set a criteria for a connector (i.e. it's transport is writable) that, once met,
 * the connector shall be placed in the driver's work queue.
 *
 * @param[in] connector The connector object to activate
 * @param[in] criteria  The criteria that must be met prior to activating the connector
 */
PN_EXTERN void pn_connector_activate(pn_connector_t *connector, pn_activate_criteria_t criteria);

/** Return the activation status of the connector for a criteria
 *
 * Return the activation status (i.e. readable, writable) for the connector.  This function
 * has the side-effect of canceling the activation of the criteria.
 *
 * Please note that this function must not be used for normal AMQP connectors.  It is only
 * used for connectors created so the driver can track non-AMQP file descriptors.  Such
 * connectors are never passed into pn_connector_process.
 *
 * @param[in] connector The connector object to activate
 * @param[in] criteria  The criteria to test.  "Is this the reason the connector appeared
 *                      in the work list?"
 * @return true iff the criteria is activated on the connector.
 */
PN_EXTERN bool pn_connector_activated(pn_connector_t *connector, pn_activate_criteria_t criteria);


#ifdef __cplusplus
}
#endif

#endif /* driver.h */
