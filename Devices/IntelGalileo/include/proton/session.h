#ifndef PROTON_SESSION_H
#define PROTON_SESSION_H 1

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
 * Session API for the proton Engine.
 *
 * @defgroup session Session
 * @ingroup engine
 * @{
 */

/**
 * Factory for creating a new session on a given connection object.
 *
 * Creates a new session object and adds it to the set of sessions
 * maintained by the connection object.
 *
 * @param[in] connection the connection object
 * @return a pointer to the new session
 */
PN_EXTERN pn_session_t *pn_session(pn_connection_t *connection);

/**
 * Free a session object.
 *
 * When a session object is freed, all ::pn_link_t, and
 * ::pn_delivery_t objects associated with the session are also
 * freed.
 *
 * @param[in] session a session object to free (or NULL)
 */
PN_EXTERN void pn_session_free(pn_session_t *session);

/**
 * Get the application context that is associated with a session
 * object.
 *
 * The application context for a session may be set using
 * ::pn_session_set_context.
 *
 * @param[in] session the session whose context is to be returned.
 * @return the application context for the session object
 */
PN_EXTERN void *pn_session_get_context(pn_session_t *session);

/**
 * Set a new application context for a session object.
 *
 * The application context for a session object may be retrieved
 * using ::pn_session_get_context.
 *
 * @param[in] session the session object
 * @param[in] context the application context
 */
PN_EXTERN void pn_session_set_context(pn_session_t *session, void *context);

/**
 * Get the endpoint state flags for a session.
 *
 * @param[in] session the session
 * @return the session's state flags
 */
PN_EXTERN pn_state_t pn_session_state(pn_session_t *session);

/**
 * Get additional error information associated with the session.
 *
 * Whenever a session operation fails (i.e. returns an error code),
 * additional error details can be obtained using this function. The
 * error object that is returned may also be used to clear the error
 * condition.
 *
 * The pointer returned by this operation is valid until the
 * session object is freed.
 *
 * @param[in] session the sesion object
 * @return the session's error object
 */
PN_EXTERN pn_error_t *pn_session_error(pn_session_t *session);

/**
 * Get the local condition associated with the session endpoint.
 *
 * The ::pn_condition_t object retrieved may be modified prior to
 * closing the session in order to indicate a particular condition
 * exists when the session closes. This is normally used to
 * communicate error conditions to the remote peer, however it may
 * also be used in non error cases. See ::pn_condition_t for more
 * details.
 *
 * The pointer returned by this operation is valid until the session
 * object is freed.
 *
 * @param[in] session the session object
 * @return the session's local condition object
 */
PN_EXTERN pn_condition_t *pn_session_condition(pn_session_t *session);

/**
 * Get the remote condition associated with the session endpoint.
 *
 * The ::pn_condition_t object retrieved may be examined in order to
 * determine whether the remote peer was indicating some sort of
 * exceptional condition when the remote session endpoint was
 * closed. The ::pn_condition_t object returned may not be modified.
 *
 * The pointer returned by this operation is valid until the
 * session object is freed.
 *
 * @param[in] session the session object
 * @return the session's remote condition object
 */
PN_EXTERN pn_condition_t *pn_session_remote_condition(pn_session_t *session);

/**
 * Get the parent connection for a session object.
 *
 * This operation retrieves the parent pn_connection_t object that
 * contains the given pn_session_t object.
 *
 * @param[in] session the session object
 * @return the parent connection object
 */
PN_EXTERN pn_connection_t *pn_session_connection(pn_session_t *session);

/**
 * Open a session.
 *
 * Once this operation has completed, the PN_LOCAL_ACTIVE state flag
 * will be set.
 *
 * @param[in] session a session object
 */
PN_EXTERN void pn_session_open(pn_session_t *session);

/**
 * Close a session.
 *
 * Once this operation has completed, the PN_LOCAL_CLOSED state flag
 * will be set. This may be called without calling
 * ::pn_session_open, in this case it is equivalent to calling
 * ::pn_session_open followed by ::pn_session_close.
 *
 * @param[in] session a session object
 */
PN_EXTERN void pn_session_close(pn_session_t *session);

/**
 * Get the incoming capacity of the session measured in bytes.
 *
 * The incoming capacity of a session determines how much incoming
 * message data the session will buffer. Note that if this value is
 * less than the negotiated frame size of the transport, it will be
 * rounded up to one full frame.
 *
 * @param[in] session the session object
 * @return the incoming capacity of the session in bytes
 */
PN_EXTERN size_t pn_session_get_incoming_capacity(pn_session_t *session);

/**
 * Set the incoming capacity for a session object.
 *
 * The incoming capacity of a session determines how much incoming
 * message data the session will buffer. Note that if this value is
 * less than the negotiated frame size of the transport, it will be
 * rounded up to one full frame.
 *
 * @param[in] session the session object
 * @param[in] capacity the incoming capacity for the session
 */
PN_EXTERN void pn_session_set_incoming_capacity(pn_session_t *session, size_t capacity);

/**
 * Get the number of outgoing bytes currently buffered by a session.
 *
 * @param[in] session a session object
 * @return the number of outgoing bytes currently buffered
 */
PN_EXTERN size_t pn_session_outgoing_bytes(pn_session_t *session);

/**
 * Get the number of incoming bytes currently buffered by a session.
 *
 * @param[in] session a session object
 * @return the number of incoming bytes currently buffered
 */
PN_EXTERN size_t pn_session_incoming_bytes(pn_session_t *session);

/**
 * Retrieve the first session from a given connection that matches the
 * specified state mask.
 *
 * Examines the state of each session owned by the connection, and
 * returns the first session that matches the given state mask. If
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

/**
 * Retrieve the next session from a given connection that matches the
 * specified state mask.
 *
 * When used with ::pn_session_head, application can access all
 * sessions on the connection that match the given state. See
 * ::pn_session_head for description of match behavior.
 *
 * @param[in] session the previous session obtained from
 *                    ::pn_session_head or ::pn_session_next
 * @param[in] state mask to match.
 * @return the next session owned by the connection that matches the
 * mask, else NULL if no sessions match
 */
PN_EXTERN pn_session_t *pn_session_next(pn_session_t *session, pn_state_t state);

/** @}
 */

#ifdef __cplusplus
}
#endif

#endif /* session.h */
