#ifndef PROTON_SSL_H
#define PROTON_SSL_H 1

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
 * API for using SSL with the Transport Layer.
 *
 * A Transport may be configured to use SSL for encryption and/or authentication.  A
 * Transport can be configured as either an "SSL client" or an "SSL server".  An SSL
 * client is the party that proactively establishes a connection to an SSL server.  An SSL
 * server is the party that accepts a connection request from a remote SSL client.
 *
 * This SSL implementation defines the following objects:

 * @li A top-level object that stores the configuration used by one or more SSL
 * sessions (pn_ssl_domain_t).
 * @li A per-connection SSL session object that performs the encryption/authentication
 * associated with the transport (pn_ssl_t).
 * @li The encryption parameters negotiated for the SSL session (pn_ssl_state_t).
 *
 * A pn_ssl_domain_t object must be created and configured before an SSL session can be
 * established.  The pn_ssl_domain_t is used to construct an SSL session (pn_ssl_t).  The
 * session "adopts" its configuration from the pn_ssl_domain_t that was used to create it.
 * For example, pn_ssl_domain_t can be configured as either a "client" or a "server".  SSL
 * sessions constructed from this domain will perform the corresponding role (either
 * client or server).
 *
 * If either an SSL server or client needs to identify itself with the remote node, it
 * must have its SSL certificate configured (see ::pn_ssl_domain_set_credentials()).
 *
 * If either an SSL server or client needs to verify the identity of the remote node, it
 * must have its database of trusted CAs configured (see ::pn_ssl_domain_set_trusted_ca_db()).
 *
 * An SSL server connection may allow the remote client to connect without SSL (eg. "in
 * the clear"), see ::pn_ssl_domain_allow_unsecured_client().
 *
 * The level of verification required of the remote may be configured (see
 * ::pn_ssl_domain_set_peer_authentication)
 *
 * Support for SSL Client Session resume is provided (see ::pn_ssl_init,
 * ::pn_ssl_resume_status).
 *
 * @defgroup ssl SSL
 * @ingroup transport
 * @{
 */

typedef struct pn_ssl_domain_t pn_ssl_domain_t;
typedef struct pn_ssl_t pn_ssl_t;

/** Determines the type of SSL endpoint. */
typedef enum {
  PN_SSL_MODE_CLIENT=1, /**< Local connection endpoint is an SSL client */
  PN_SSL_MODE_SERVER    /**< Local connection endpoint is an SSL server */
} pn_ssl_mode_t;

/** Indicates whether an SSL session has been resumed. */
typedef enum {
  PN_SSL_RESUME_UNKNOWN,        /**< Session resume state unknown/not supported */
  PN_SSL_RESUME_NEW,            /**< Session renegotiated - not resumed */
  PN_SSL_RESUME_REUSED          /**< Session resumed from previous session. */
} pn_ssl_resume_status_t;

/** Create an SSL configuration domain
 *
 * This method allocates an SSL domain object.  This object is used to hold the SSL
 * configuration for one or more SSL sessions.  The SSL session object (pn_ssl_t) is
 * allocated from this object.
 *
 * @param[in] mode the role, client or server, assumed by all SSL sessions created
 * with this domain.
 * @return a pointer to the SSL domain, if SSL support is present.
 */
PN_EXTERN pn_ssl_domain_t *pn_ssl_domain( pn_ssl_mode_t mode);

/** Release an SSL configuration domain
 *
 * This method frees an SSL domain object allocated by ::pn_ssl_domain.
 * @param[in] domain the domain to destroy.
 */
PN_EXTERN void pn_ssl_domain_free( pn_ssl_domain_t *domain );

/** Set the certificate that identifies the local node to the remote.
 *
 * This certificate establishes the identity for the local node for all SSL sessions
 * created from this domain.  It will be sent to the remote if the remote needs to verify
 * the identity of this node.  This may be used for both SSL servers and SSL clients (if
 * client authentication is required by the server).
 *
 * @note This setting effects only those pn_ssl_t objects created after this call
 * returns.  pn_ssl_t objects created before invoking this method will use the domain's
 * previous setting.
 *
 * @param[in] domain the ssl domain that will use this certificate.
 * @param[in] certificate_file path to file/database containing the identifying
 * certificate.
 * @param[in] private_key_file path to file/database containing the private key used to
 * sign the certificate
 * @param[in] password the password used to sign the key, else NULL if key is not
 * protected.
 * @return 0 on success
 */
PN_EXTERN int pn_ssl_domain_set_credentials( pn_ssl_domain_t *domain,
                                             const char *certificate_file,
                                             const char *private_key_file,
                                             const char *password);

/** Configure the set of trusted CA certificates used by this domain to verify peers.
 *
 * If the local SSL client/server needs to verify the identity of the remote, it must
 * validate the signature of the remote's certificate.  This function sets the database of
 * trusted CAs that will be used to verify the signature of the remote's certificate.
 *
 * @note This setting effects only those pn_ssl_t objects created after this call
 * returns.  pn_ssl_t objects created before invoking this method will use the domain's
 * previous setting.
 *
 * @param[in] domain the ssl domain that will use the database.
 * @param[in] certificate_db database of trusted CAs, used to authenticate the peer.
 * @return 0 on success
 */
PN_EXTERN int pn_ssl_domain_set_trusted_ca_db(pn_ssl_domain_t *domain,
                                const char *certificate_db);

/** Determines the level of peer validation.
 *
 *  ANONYMOUS_PEER does not require a valid certificate, and permits use of ciphers that
 *  do not provide authentication.
 *
 *  VERIFY_PEER will only connect to those peers that provide a valid identifying
 *  certificate signed by a trusted CA and are using an authenticated cipher.
 *
 *  VERIFY_PEER_NAME is like VERIFY_PEER, but also requires the peer's identity as
 *  contained in the certificate to be valid (see ::pn_ssl_set_peer_hostname).
 *
 *  ANONYMOUS_PEER is configured by default.
 */
typedef enum {
  PN_SSL_VERIFY_NULL=0,   /**< internal use only */
  PN_SSL_VERIFY_PEER,     /**< require peer to provide a valid identifying certificate */
  PN_SSL_ANONYMOUS_PEER,  /**< do not require a certificate nor cipher authorization */
  PN_SSL_VERIFY_PEER_NAME /**< require valid certificate and matching name */
} pn_ssl_verify_mode_t;

/** Configure the level of verification used on the peer certificate.
 *
 * This method controls how the peer's certificate is validated, if at all.  By default,
 * neither servers nor clients attempt to verify their peers (PN_SSL_ANONYMOUS_PEER).
 * Once certificates and trusted CAs are configured, peer verification can be enabled.
 *
 * @note In order to verify a peer, a trusted CA must be configured. See
 * ::pn_ssl_domain_set_trusted_ca_db().
 *
 * @note Servers must provide their own certificate when verifying a peer.  See
 * ::pn_ssl_domain_set_credentials().
 *
 * @note This setting effects only those pn_ssl_t objects created after this call
 * returns.  pn_ssl_t objects created before invoking this method will use the domain's
 * previous setting.
 *
 * @param[in] domain the ssl domain to configure.
 * @param[in] mode the level of validation to apply to the peer
 * @param[in] trusted_CAs path to a database of trusted CAs that the server will advertise
 * to the peer client if the server has been configured to verify its peer.
 * @return 0 on success
 */
PN_EXTERN int pn_ssl_domain_set_peer_authentication(pn_ssl_domain_t *domain,
                                          const pn_ssl_verify_mode_t mode,
                                          const char *trusted_CAs);

/** Permit a server to accept connection requests from non-SSL clients.
 *
 * This configures the server to "sniff" the incoming client data stream, and dynamically
 * determine whether SSL/TLS is being used.  This option is disabled by default: only
 * clients using SSL/TLS are accepted.
 *
 * @param[in] domain the domain (server) that will accept the client connections.
 * @return 0 on success
 */
PN_EXTERN int pn_ssl_domain_allow_unsecured_client(pn_ssl_domain_t *domain);

/** Create a new SSL session object associated with a transport.
 *
 * A transport must have an SSL object in order to "speak" SSL over its connection. This
 * method allocates an SSL object associates it with the transport.
 *
 * @param[in] transport the transport that will own the new SSL session.
 * @return a pointer to the SSL object configured for this transport.  Returns NULL if
 * no SSL session is associated with the transport.
 */
PN_EXTERN pn_ssl_t *pn_ssl(pn_transport_t *transport);

/** Initialize an SSL session.
 *
 * This method configures an SSL object using the configuration provided by the given
 * domain.
 *
 * @param[in] ssl the ssl session to configured.
 * @param[in] domain the ssl domain used to configure the SSL session.
 * @param[in] session_id if supplied, attempt to resume a previous SSL
 * session that used the same session_id.  If no previous SSL session
 * is available, a new session will be created using the session_id
 * and stored for future session restore (see ::::pn_ssl_resume_status).
 * @return 0 on success, else an error code.
 */
PN_EXTERN int pn_ssl_init( pn_ssl_t *ssl,
                 pn_ssl_domain_t *domain,
                 const char *session_id);

/** Get the name of the Cipher that is currently in use.
 *
 * Gets a text description of the cipher that is currently active, or returns FALSE if SSL
 * is not active (no cipher).  Note that the cipher in use may change over time due to
 * renegotiation or other changes to the SSL state.
 *
 * @param[in] ssl the ssl client/server to query.
 * @param[in,out] buffer buffer of size bytes to hold cipher name
 * @param[in] size maximum number of bytes in buffer.
 * @return True if cipher name written to buffer, False if no cipher in use.
 */
PN_EXTERN bool pn_ssl_get_cipher_name(pn_ssl_t *ssl, char *buffer, size_t size);

/** Get the name of the SSL protocol that is currently in use.
 *
 * Gets a text description of the SSL protocol that is currently active, or returns FALSE if SSL
 * is not active.  Note that the protocol may change over time due to renegotiation.
 *
 * @param[in] ssl the ssl client/server to query.
 * @param[in,out] buffer buffer of size bytes to hold the version identifier
 * @param[in] size maximum number of bytes in buffer.
 * @return True if the version information was written to buffer, False if SSL connection
 * not ready.
 */
PN_EXTERN bool pn_ssl_get_protocol_name(pn_ssl_t *ssl, char *buffer, size_t size);

/** Check whether the state has been resumed.
 *
 * Used for client session resume.  When called on an active session, indicates whether
 * the state has been resumed from a previous session.
 *
 * @note This is a best-effort service - there is no guarantee that the remote server will
 * accept the resumed parameters.  The remote server may choose to ignore these
 * parameters, and request a re-negotiation instead.
 *
 * @param[in] ssl the ssl session to check
 * @return status code indicating whether or not the session has been resumed.
 */
PN_EXTERN pn_ssl_resume_status_t pn_ssl_resume_status( pn_ssl_t *ssl );

/** Set the expected identity of the remote peer.
 *
 * The hostname is used for two purposes: 1) when set on an SSL client, it is sent to the
 * server during the handshake (if Server Name Indication is supported), and 2) it is used
 * to check against the identifying name provided in the peer's certificate. If the
 * supplied name does not exactly match a SubjectAltName (type DNS name), or the
 * CommonName entry in the peer's certificate, the peer is considered unauthenticated
 * (potential imposter), and the SSL connection is aborted.
 *
 * @note Verification of the hostname is only done if PN_SSL_VERIFY_PEER_NAME is enabled.
 * See ::pn_ssl_domain_set_peer_authentication.
 *
 * @param[in] ssl the ssl session.
 * @param[in] hostname the expected identity of the remote. Must conform to the syntax as
 * given in RFC1034, Section 3.5.
 * @return 0 on success.
 */
PN_EXTERN int pn_ssl_set_peer_hostname( pn_ssl_t *ssl, const char *hostname);


/** Access the configured peer identity.
 *
 * Return the expected identity of the remote peer, as set by ::pn_ssl_set_peer_hostname.
 *
 * @param[in] ssl the ssl session.
 * @param[out] hostname buffer to hold the null-terminated name string. If null, no string
 * is written.
 * @param[in,out] bufsize on input set to the number of octets in hostname. On output, set
 * to the number of octets needed to hold the value of hostname plus a null byte.  Zero if
 * no hostname set.
 * @return 0 on success.
 */
PN_EXTERN int pn_ssl_get_peer_hostname( pn_ssl_t *ssl, char *hostname, size_t *bufsize );

/** @} */

#ifdef __cplusplus
}
#endif

#endif /* ssl.h */
