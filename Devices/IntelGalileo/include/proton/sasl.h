#ifndef PROTON_SASL_H
#define PROTON_SASL_H 1

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
#include <sys/types.h>
#include <proton/type_compat.h>
#include <proton/types.h>

#ifdef __cplusplus
extern "C" {
#endif

/** @file
 * API for the SASL Secure Transport Layer.
 *
 * The SASL layer is responsible for establishing an authenticated
 * and/or encrypted tunnel over which AMQP frames are passed between
 * peers. The peer acting as the SASL Client must provide
 * authentication credentials. The peer acting as the SASL Server must
 * provide authentication against the received credentials.
 *
 * @defgroup sasl SASL
 * @ingroup transport
 * @{
 */

typedef struct pn_sasl_t pn_sasl_t;

/** The result of the SASL negotiation */
typedef enum {
  PN_SASL_NONE=-1,  /** negotiation not completed */
  PN_SASL_OK=0,     /** authentication succeeded */
  PN_SASL_AUTH=1,   /** failed due to bad credentials */
  PN_SASL_SYS=2,    /** failed due to a system error */
  PN_SASL_PERM=3,   /** failed due to unrecoverable error */
  PN_SASL_TEMP=4,   /** failed due to transient error */
  PN_SASL_SKIPPED=5 /** the peer didn't perform the sasl exchange */
} pn_sasl_outcome_t;

/** The state of the SASL negotiation process */
typedef enum {
  PN_SASL_CONF,    /** Pending configuration by application */
  PN_SASL_IDLE,    /** Pending SASL Init */
  PN_SASL_STEP,    /** negotiation in progress */
  PN_SASL_PASS,    /** negotiation completed successfully */
  PN_SASL_FAIL     /** negotiation failed */
} pn_sasl_state_t;

/** Construct an Authentication and Security Layer object
 *
 * @return a new SASL object representing the layer.
 */
PN_EXTERN pn_sasl_t *pn_sasl(pn_transport_t *transport);

/** Access the current state of the layer.
 *
 * @param[in] sasl the layer to retrieve the state from.
 * @return The state of the sasl layer.
 */
PN_EXTERN pn_sasl_state_t pn_sasl_state(pn_sasl_t *sasl);

/** Set the acceptable SASL mechanisms for the layer.
 *
 * @param[in] sasl the layer to update
 * @param[in] mechanisms a list of acceptable SASL mechanisms,
 *                       separated by space
 */
PN_EXTERN void pn_sasl_mechanisms(pn_sasl_t *sasl, const char *mechanisms);

/** Retrieve the list of SASL mechanisms provided by the remote.
 *
 * @param[in] sasl the SASL layer.
 * @return a string containing a list of the SASL mechanisms
 *         advertised by the remote (separated by spaces)
 */
PN_EXTERN const char *pn_sasl_remote_mechanisms(pn_sasl_t *sasl);

/** Configure the SASL layer to act as a SASL client.
 *
 * The role of client is similar to a TCP client - the peer requesting
 * the connection.
 *
 * @param[in] sasl the SASL layer to configure as a client
 */
PN_EXTERN void pn_sasl_client(pn_sasl_t *sasl);

/** Configure the SASL layer to act as a server.
 *
 * The role of server is similar to a TCP server - the peer accepting
 * the connection.
 *
 * @param[in] sasl the SASL layer to configure as a server
 */
PN_EXTERN void pn_sasl_server(pn_sasl_t *sasl);

/** Configure a SASL server layer to permit the client to skip the SASL exchange.
 *
 * If the peer client skips the SASL exchange (i.e. goes right to the AMQP header)
 * this server layer will succeed and result in the outcome of PN_SASL_SKIPPED.
 * The default behavior is to fail and close the connection if the client skips
 * SASL.
 *
 * @param[in] sasl the SASL layer to configure
 * @param[in] allow true -> allow skip; false -> forbid skip
 */
    PN_EXTERN void pn_sasl_allow_skip(pn_sasl_t *sasl, bool allow);

/** Configure the SASL layer to use the "PLAIN" mechanism.
 *
 * A utility function to configure a simple client SASL layer using
 * PLAIN authentication.
 *
 * @param[in] sasl the layer to configure.
 * @param[in] username credential for the PLAIN authentication
 *                     mechanism
 * @param[in] password credential for the PLAIN authentication
 *                     mechanism
 */
PN_EXTERN void pn_sasl_plain(pn_sasl_t *sasl, const char *username, const char *password);

/** Determine the size of the bytes available via pn_sasl_recv().
 *
 * Returns the size in bytes available via pn_sasl_recv().
 *
 * @param[in] sasl the SASL layer.
 * @return The number of bytes available, zero if no available data.
 */
PN_EXTERN size_t pn_sasl_pending(pn_sasl_t *sasl);

/** Read challenge/response data sent from the peer.
 *
 * Use pn_sasl_pending to determine the size of the data.
 *
 * @param[in] sasl the layer to read from.
 * @param[out] bytes written with up to size bytes of inbound data.
 * @param[in] size maximum number of bytes that bytes can accept.
 * @return The number of bytes written to bytes, or an error code if < 0.
 */
PN_EXTERN ssize_t pn_sasl_recv(pn_sasl_t *sasl, char *bytes, size_t size);

/** Send challenge or response data to the peer.
 *
 * @param[in] sasl The SASL layer.
 * @param[in] bytes The challenge/response data.
 * @param[in] size The number of data octets in bytes.
 * @return The number of octets read from bytes, or an error code if < 0
 */
PN_EXTERN ssize_t pn_sasl_send(pn_sasl_t *sasl, const char *bytes, size_t size);

/** Set the outcome of SASL negotiation
 *
 * Used by the server to set the result of the negotiation process.
 *
 * @todo
 */
PN_EXTERN void pn_sasl_done(pn_sasl_t *sasl, pn_sasl_outcome_t outcome);

/** Retrieve the outcome of SASL negotiation.
 *
 * @todo
 */
PN_EXTERN pn_sasl_outcome_t pn_sasl_outcome(pn_sasl_t *sasl);

/** @} */

#ifdef __cplusplus
}
#endif

#endif /* sasl.h */
