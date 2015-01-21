#ifndef PROTON_CONNECTION_H
#define PROTON_CONNECTION_H 1

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
#include <proton/condition.h>
#include <proton/error.h>
#include <proton/type_compat.h>
#include <proton/types.h>

#include <stddef.h>
#include <sys/types.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * @file
 *
 * Connection API for the proton Engine.
 *
 * @defgroup connection Connection
 * @ingroup engine
 * @{
 */

/**
 * The local @link pn_state_t endpoint state @endlink is uninitialized.
 */
#define PN_LOCAL_UNINIT (1)
/**
 * The local @link pn_state_t endpoint state @endlink is active.
 */
#define PN_LOCAL_ACTIVE (2)
/**
 * The local @link pn_state_t endpoint state @endlink is closed.
 */
#define PN_LOCAL_CLOSED (4)
/**
 * The remote @link pn_state_t endpoint state @endlink is uninitialized.
 */
#define PN_REMOTE_UNINIT (8)
/**
 * The remote @link pn_state_t endpoint state @endlink is active.
 */
#define PN_REMOTE_ACTIVE (16)
/**
 * The remote @link pn_state_t endpoint state @endlink is closed.
 */
#define PN_REMOTE_CLOSED (32)

/**
 * A mask for values of ::pn_state_t that preserves only the local
 * bits of an endpoint's state.
 */
#define PN_LOCAL_MASK (PN_LOCAL_UNINIT | PN_LOCAL_ACTIVE | PN_LOCAL_CLOSED)

/**
 * A mask for values of ::pn_state_t that preserves only the remote
 * bits of an endpoint's state.
 */
#define PN_REMOTE_MASK (PN_REMOTE_UNINIT | PN_REMOTE_ACTIVE | PN_REMOTE_CLOSED)

/**
 * Factory to construct a new Connection.
 *
 * @return pointer to a new connection object.
 */
PN_EXTERN pn_connection_t *pn_connection(void);

/**
 * Free a connection object.
 *
 * When a connection object is freed, all ::pn_session_t, ::pn_link_t,
 * and ::pn_delivery_t objects associated with the connection are also
 * freed.
 *
 * @param[in] connection the connection object to free (or NULL)
 */
PN_EXTERN void pn_connection_free(pn_connection_t *connection);

/**
 * Get additional error information associated with the connection.
 *
 * Whenever a connection operation fails (i.e. returns an error code),
 * additional error details can be obtained using this function. The
 * error object that is returned may also be used to clear the error
 * condition.
 *
 * The pointer returned by this operation is valid until the
 * connection object is freed.
 *
 * @param[in] connection the connection object
 * @return the connection's error object
 */
PN_EXTERN pn_error_t *pn_connection_error(pn_connection_t *connection);

/**
 * Associate a connection object with an event collector.
 *
 * By associating a connection object with an event collector, key
 * changes in endpoint state are reported to the collector via
 * ::pn_event_t objects that can be inspected and processed. See
 * ::pn_event_t for more details on the kinds of events.
 *
 * Note that by registering a collector, the user is requesting that
 * an indefinite number of events be queued up on his behalf. This
 * means that unless the application eventually processes these
 * events, the storage requirements for keeping them will grow without
 * bound. In other words, don't register a collector with a connection
 * if you never intend to process any of the events.
 *
 * @param[in] connection the connection object
 * @param[in] collector the event collector
 */
PN_EXTERN void pn_connection_collect(pn_connection_t *connection, pn_collector_t *collector);

/**
 * Get the application context that is associated with a connection
 * object.
 *
 * The application context for a connection may be set using
 * ::pn_connection_set_context.
 *
 * @param[in] connection the connection whose context is to be returned.
 * @return the application context for the connection object
 */
PN_EXTERN void *pn_connection_get_context(pn_connection_t *connection);

/**
 * Set a new application context for a connection object.
 *
 * The application context for a connection object may be retrieved
 * using ::pn_connection_get_context.
 *
 * @param[in] connection the connection object
 * @param[in] context the application context
 */
PN_EXTERN void pn_connection_set_context(pn_connection_t *connection, void *context);

/**
 * Get the endpoint state flags for a connection.
 *
 * @param[in] connection the connection
 * @return the connection's state flags
 */
PN_EXTERN pn_state_t pn_connection_state(pn_connection_t *connection);

/**
 * Open a connection.
 *
 * Once this operation has completed, the PN_LOCAL_ACTIVE state flag
 * will be set.
 *
 * @param[in] connection a connection object
 */
PN_EXTERN void pn_connection_open(pn_connection_t *connection);

/**
 * Close a connection.
 *
 * Once this operation has completed, the PN_LOCAL_CLOSED state flag
 * will be set. This may be called without calling
 * ::pn_connection_open, in this case it is equivalent to calling
 * ::pn_connection_open followed by ::pn_connection_close.
 *
 * @param[in] connection a connection object
 */
PN_EXTERN void pn_connection_close(pn_connection_t *connection);

/**
 * Reset a connection object back to the uninitialized state.
 *
 * Note that this does *not* remove any contained ::pn_session_t,
 * ::pn_link_t, and ::pn_delivery_t objects.
 *
 * @param[in] connection a connection object
 */
PN_EXTERN void pn_connection_reset(pn_connection_t *connection);

/**
 * Get the local condition associated with the connection endpoint.
 *
 * The ::pn_condition_t object retrieved may be modified prior to
 * closing the connection in order to indicate a particular condition
 * exists when the connection closes. This is normally used to
 * communicate error conditions to the remote peer, however it may
 * also be used in non error cases such as redirects. See
 * ::pn_condition_t for more details.
 *
 * The pointer returned by this operation is valid until the
 * connection object is freed.
 *
 * @param[in] connection the connection object
 * @return the connection's local condition object
 */
PN_EXTERN pn_condition_t *pn_connection_condition(pn_connection_t *connection);

/**
 * Get the remote condition associated with the connection endpoint.
 *
 * The ::pn_condition_t object retrieved may be examined in order to
 * determine whether the remote peer was indicating some sort of
 * exceptional condition when the remote connection endpoint was
 * closed. The ::pn_condition_t object returned may not be modified.
 *
 * The pointer returned by this operation is valid until the
 * connection object is freed.
 *
 * @param[in] connection the connection object
 * @return the connection's remote condition object
 */
PN_EXTERN pn_condition_t *pn_connection_remote_condition(pn_connection_t *connection);

/**
 * Get the AMQP Container name advertised by a connection object.
 *
 * The pointer returned by this operation is valid until
 * ::pn_connection_set_container is called, or until the connection
 * object is freed, whichever happens sooner.
 *
 * @param[in] connection the connection object
 * @return a pointer to the container name
 */
PN_EXTERN const char *pn_connection_get_container(pn_connection_t *connection);

/**
 * Set the AMQP Container name advertised by a connection object.
 *
 * @param[in] connection the connection object
 * @param[in] container the container name
 */
PN_EXTERN void pn_connection_set_container(pn_connection_t *connection, const char *container);

/**
 * Get the value of the AMQP Hostname used by a connection object.
 *
 * The pointer returned by this operation is valid until
 * ::pn_connection_set_hostname is called, or until the connection
 * object is freed, whichever happens sooner.
 *
 * @param[in] connection the connection object
 * @return a pointer to the hostname
 */
PN_EXTERN const char *pn_connection_get_hostname(pn_connection_t *connection);

/**
 * Set the value of the AMQP Hostname used by a connection object.
 *
 * @param[in] connection the connection object
 * @param[in] hostname the hostname
 */
PN_EXTERN void pn_connection_set_hostname(pn_connection_t *connection, const char *hostname);

/**
 * Get the AMQP Container name advertised by the remote connection
 * endpoint.
 *
 * This will return NULL until the ::PN_REMOTE_ACTIVE state is
 * reached. See ::pn_state_t for more details on endpoint state.
 *
 * Any non null pointer returned by this operation will be valid until
 * the connection object is unbound from a transport or freed,
 * whichever happens sooner.
 *
 * @param[in] connection the connection object
 * @return a pointer to the remote container name
 */
PN_EXTERN const char *pn_connection_remote_container(pn_connection_t *connection);

/**
 * Get the AMQP Hostname set by the remote connection endpoint.
 *
 * This will return NULL until the ::PN_REMOTE_ACTIVE state is
 * reached. See ::pn_state_t for more details on endpoint state.
 *
 * Any non null pointer returned by this operation will be valid until
 * the connection object is unbound from a transport or freed,
 * whichever happens sooner.
 *
 * @param[in] connection the connection object
 * @return a pointer to the remote hostname
 */
PN_EXTERN const char *pn_connection_remote_hostname(pn_connection_t *connection);

/**
 * Access/modify the AMQP offered capabilities data for a connection
 * object.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the connection object is freed. Any data contained
 * by the ::pn_data_t object will be sent as the offered capabilites
 * for the parent connection object. Note that this MUST take the form
 * of an array of symbols to be valid.
 *
 * The ::pn_data_t pointer returned is valid until the connection
 * object is freed.
 *
 * @param[in] connection the connection object
 * @return a pointer to a pn_data_t representing the offered capabilities
 */
PN_EXTERN pn_data_t *pn_connection_offered_capabilities(pn_connection_t *connection);

/**
 * Access/modify the AMQP desired capabilities data for a connection
 * object.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the connection object is freed. Any data contained
 * by the ::pn_data_t object will be sent as the desired capabilites
 * for the parent connection object. Note that this MUST take the form
 * of an array of symbols to be valid.
 *
 * The ::pn_data_t pointer returned is valid until the connection
 * object is freed.
 *
 * @param[in] connection the connection object
 * @return a pointer to a pn_data_t representing the desired capabilities
 */
PN_EXTERN pn_data_t *pn_connection_desired_capabilities(pn_connection_t *connection);

/**
 * Access/modify the AMQP properties data for a connection object.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the connection object is freed. Any data contained
 * by the ::pn_data_t object will be sent as the AMQP properties for
 * the parent connection object. Note that this MUST take the form of
 * a symbol keyed map to be valid.
 *
 * The ::pn_data_t pointer returned is valid until the connection
 * object is freed.
 *
 * @param[in] connection the connection object
 * @return a pointer to a pn_data_t representing the connection properties
 */
PN_EXTERN pn_data_t *pn_connection_properties(pn_connection_t *connection);

/**
 * Access the AMQP offered capabilites supplied by the remote
 * connection endpoint.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the connection object is freed. This data object
 * will be empty until the remote connection is opened as indicated by
 * the ::PN_REMOTE_ACTIVE flag.
 *
 * @param[in] connection the connection object
 * @return the remote offered capabilities
 */
PN_EXTERN pn_data_t *pn_connection_remote_offered_capabilities(pn_connection_t *connection);

/**
 * Access the AMQP desired capabilites supplied by the remote
 * connection endpoint.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the connection object is freed. This data object
 * will be empty until the remote connection is opened as indicated by
 * the ::PN_REMOTE_ACTIVE flag.
 *
 * @param[in] connection the connection object
 * @return the remote desired capabilities
 */
PN_EXTERN pn_data_t *pn_connection_remote_desired_capabilities(pn_connection_t *connection);

/**
 * Access the AMQP connection properties supplied by the remote
 * connection endpoint.
 *
 * This operation will return a pointer to a ::pn_data_t object that
 * is valid until the connection object is freed. This data object
 * will be empty until the remote connection is opened as indicated by
 * the ::PN_REMOTE_ACTIVE flag.
 *
 * @param[in] connection the connection object
 * @return the remote connection properties
 */
PN_EXTERN pn_data_t *pn_connection_remote_properties(pn_connection_t *connection);

/**
 * Get the transport bound to a connection object.
 *
 * If the connection is unbound, then this operation will return NULL.
 *
 * @param[in] connection the connection object
 * @return the transport bound to a connection, or NULL if the
 * connection is unbound
 */
PN_EXTERN pn_transport_t *pn_connection_transport(pn_connection_t *connection);

/** @}
 */

#ifdef __cplusplus
}
#endif

#endif /* connection.h */
