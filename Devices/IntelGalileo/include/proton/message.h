#ifndef PROTON_MESSAGE_H
#define PROTON_MESSAGE_H 1

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
#include <proton/types.h>
#include <proton/codec.h>
#include <proton/error.h>
#include <sys/types.h>
#include <proton/type_compat.h>

#ifdef __cplusplus
extern "C" {
#endif

/** @file
 * Message API for encoding/decoding AMQP Messages.
 *
 * @defgroup message Message
 * @{
 */

/**
 * An AMQP Message object.
 *
 * An AMQP Message object is a mutable holder of message content that
 * may be used to generate and encode or decode and access AMQP
 * formatted message data.
 */
typedef struct pn_message_t pn_message_t;

/**
 * Encoding format for message content.
 */
typedef enum {
  PN_DATA, /**< Raw binary data. Not all messages can be encoded this way.*/
  PN_TEXT, /**< Raw text. Not all messages can be encoded this way.*/
  PN_AMQP, /**< AMQP formatted data. All messages can be encoded this way.*/
  PN_JSON /**< JSON formatted data. Not all messages can be encoded with full fidelity way.*/
} pn_format_t;

/**
 * Default priority for messages.
 */
#define PN_DEFAULT_PRIORITY (4)

/**
 * Construct a new ::pn_message_t.
 *
 * Every message that is constructed must be freed using
 * ::pn_message_free().
 *
 * @return pointer to a new ::pn_message_t
 */
PN_EXTERN pn_message_t * pn_message(void);

/**
 * Free a previously constructed ::pn_message_t.
 *
 * @param[in] msg pointer to a ::pn_message_t or NULL
 */
PN_EXTERN void           pn_message_free(pn_message_t *msg);

/**
 * Clears the content of a ::pn_message_t.
 *
 * When pn_message_clear returns, the supplied ::pn_message_t will be
 * emptied of all content and effectively returned to the same state
 * as if it was just created.
 *
 * @param[in] msg pointer to the ::pn_message_t to be cleared
 */
PN_EXTERN void           pn_message_clear(pn_message_t *msg);

/**
 * Access the error code of a message.
 *
 * Every operation on a message that can result in an error will set
 * the message's error code in case of error. The pn_message_errno()
 * call will access the error code of the most recent failed
 * operation.
 *
 * @param[in] msg a message
 * @return the message's error code
 */
PN_EXTERN int            pn_message_errno(pn_message_t *msg);

/**
 * Access the error information for a message.
 *
 * Every operation on a message that can result in an error will
 * update the error information held by its error descriptor should
 * that operation fail. The pn_message_error() call will access the
 * error information of the most recent failed operation. The pointer
 * returned by this call is valid until the message is freed.
 *
 * @param[in] msg a message
 * @return the message's error descriptor
 */
PN_EXTERN pn_error_t    *pn_message_error(pn_message_t *msg);

/**
 * Get the inferred flag for a message.
 *
 * The inferred flag for a message indicates how the message content
 * is encoded into AMQP sections. If inferred is true then binary and
 * list values in the body of the message will be encoded as AMQP DATA
 * and AMQP SEQUENCE sections, respectively. If inferred is false,
 * then all values in the body of the message will be encoded as AMQP
 * VALUE sections regardless of their type. Use
 * ::pn_message_set_inferred to set the value.
 *
 * @param[in] msg a message object
 * @return the value of the inferred flag for the message
 */
PN_EXTERN bool           pn_message_is_inferred(pn_message_t *msg);

/**
 * Set the inferred flag for a message.
 *
 * See ::pn_message_is_inferred() for a description of what the
 * inferred flag is.
 *
 * @param[in] msg a message object
 * @param[in] inferred the new value of the inferred flag
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_inferred(pn_message_t *msg, bool inferred);

// standard message headers and properties

/**
 * Get the durable flag for a message.
 *
 * The durable flag indicates that any parties taking responsibility
 * for the message must durably store the content.
 *
 * @param[in] msg a message object
 * @return the value of the durable flag
 */
PN_EXTERN bool           pn_message_is_durable            (pn_message_t *msg);

/**
 * Set the durable flag for a message.
 *
 * See ::pn_message_is_durable() for a description of the durable
 * flag.
 *
 * @param[in] msg a message object
 * @param[in] durable the new value of the durable flag
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_durable           (pn_message_t *msg, bool durable);

/**
 * Get the priority for a message.
 *
 * The priority of a message impacts ordering guarantees. Within a
 * given ordered context, higher priority messages may jump ahead of
 * lower priority messages.
 *
 * @param[in] msg a message object
 * @return the message priority
 */
PN_EXTERN uint8_t        pn_message_get_priority          (pn_message_t *msg);

/**
 * Set the priority for a message.
 *
 * See ::pn_message_get_priority() for details on message priority.
 *
 * @param[in] msg a message object
 * @param[in] priority the new priority for the message
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_priority          (pn_message_t *msg, uint8_t priority);

/**
 * Get the ttl for a message.
 *
 * The ttl for a message determines how long a message is considered
 * live. When a message is held for retransmit, the ttl is
 * decremented. Once the ttl reaches zero, the message is considered
 * dead. Once a message is considered dead it may be dropped. Use
 * ::pn_message_set_ttl() to set the ttl for a message.
 *
 * @param[in] msg a message object
 * @return the ttl in milliseconds
 */
PN_EXTERN pn_millis_t    pn_message_get_ttl               (pn_message_t *msg);

/**
 * Set the ttl for a message.
 *
 * See ::pn_message_get_ttl() for a detailed description of message ttl.
 *
 * @param[in] msg a message object
 * @param[in] ttl the new value for the message ttl
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_ttl               (pn_message_t *msg, pn_millis_t ttl);

/**
 * Get the first acquirer flag for a message.
 *
 * When set to true, the first acquirer flag for a message indicates
 * that the recipient of the message is the first recipient to acquire
 * the message, i.e. there have been no failed delivery attempts to
 * other acquirers. Note that this does not mean the message has not
 * been delivered to, but not acquired, by other recipients.
 *
 * @param[in] msg a message object
 * @return the first acquirer flag for the message
 */
PN_EXTERN bool           pn_message_is_first_acquirer     (pn_message_t *msg);

/**
 * Set the first acquirer flag for a message.
 *
 * See ::pn_message_is_first_acquirer() for details on the first
 * acquirer flag.
 *
 * @param[in] msg a message object
 * @param[in] first the new value for the first acquirer flag
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_first_acquirer    (pn_message_t *msg, bool first);

/**
 * Get the delivery count for a message.
 *
 * The delivery count field tracks how many attempts have been made to
 * delivery a message. Use ::pn_message_set_delivery_count() to set
 * the delivery count for a message.
 *
 * @param[in] msg a message object
 * @return the delivery count for the message
 */
PN_EXTERN uint32_t       pn_message_get_delivery_count    (pn_message_t *msg);

/**
 * Set the delivery count for a message.
 *
 * See ::pn_message_get_delivery_count() for details on what the
 * delivery count means.
 *
 * @param[in] msg a message object
 * @param[in] count the new delivery count
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_delivery_count    (pn_message_t *msg, uint32_t count);

/**
 * Get/set the id for a message.
 *
 * The message id provides a globally unique identifier for a message.
 * A message id can be an a string, an unsigned long, a uuid or a
 * binary value. This operation returns a pointer to a ::pn_data_t
 * that can be used to access and/or modify the value of the message
 * id. The pointer is valid until the message is freed. See
 * ::pn_data_t for details on how to get/set the value.
 *
 * @param[in] msg a message object
 * @return pointer to a ::pn_data_t holding the id
 */
PN_EXTERN pn_data_t *    pn_message_id                    (pn_message_t *msg);

/**
 * Get the id for a message.
 *
 * The message id provides a globally unique identifier for a message.
 * A message id can be an a string, an unsigned long, a uuid or a
 * binary value. This operation returns the value of the id using the
 * ::pn_atom_t descriminated union. See ::pn_atom_t for details on how
 * to access the value.
 *
 * @param[in] msg a message object
 * @return the message id
 */
PN_EXTERN pn_atom_t      pn_message_get_id                (pn_message_t *msg);

/**
 * Set the id for a message.
 *
 * See ::pn_message_get_id() for more details on the meaning of the
 * message id. Note that only string, unsigned long, uuid, or binary
 * values are permitted.
 *
 * @param[in] msg a message object
 * @param[in] id the new value of the message id
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_id                (pn_message_t *msg, pn_atom_t id);

/**
 * Get the user id for a message.
 *
 * The pointer referenced by the ::pn_bytes_t struct will be valid
 * until any one of the following operations occur:
 *
 *  - ::pn_message_free()
 *  - ::pn_message_clear()
 *  - ::pn_message_set_user_id()
 *
 * @param[in] msg a message object
 * @return a pn_bytes_t referencing the message's user_id
 */
PN_EXTERN pn_bytes_t     pn_message_get_user_id           (pn_message_t *msg);

/**
 * Set the user id for a message.
 *
 * This operation copies the bytes referenced by the provided
 * ::pn_bytes_t struct.
 *
 * @param[in] msg a message object
 * @param[in] user_id the new user_id for the message
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_user_id           (pn_message_t *msg, pn_bytes_t user_id);

/**
 * Get the address for a message.
 *
 * This operation will return NULL if no address has been set or if
 * the address has been set to NULL. The pointer returned by this
 * operation is valid until any one of the following operations occur:
 *
 *  - ::pn_message_free()
 *  - ::pn_message_clear()
 *  - ::pn_message_set_address()
 *
 * @param[in] msg a message object
 * @return a pointer to the address of the message (or NULL)
 */
PN_EXTERN const char *   pn_message_get_address           (pn_message_t *msg);

/**
 * Set the address for a message.
 *
 * The supplied address pointer must either be NULL or reference a NUL
 * terminated string. When the pointer is NULL, the address of the
 * message is set to NULL. When the pointer is non NULL, the contents
 * are copied into the message.
 *
 * @param[in] msg a message object
 * @param[in] address a pointer to the new address (or NULL)
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_address           (pn_message_t *msg, const char *address);

/**
 * Get the subject for a message.
 *
 * This operation will return NULL if no subject has been set or if
 * the subject has been set to NULL. The pointer returned by this
 * operation is valid until any one of the following operations occur:
 *
 *  - ::pn_message_free()
 *  - ::pn_message_clear()
 *  - ::pn_message_set_subject()
 *
 * @param[in] msg a message object
 * @return a pointer to the subject of the message (or NULL)
 */
PN_EXTERN const char *   pn_message_get_subject           (pn_message_t *msg);

/**
 * Set the subject for a message.
 *
 * The supplied subject pointer must either be NULL or reference a NUL
 * terminated string. When the pointer is NULL, the subject is set to
 * NULL. When the pointer is non NULL, the contents are copied into
 * the message.
 *
 * @param[in] msg a message object
 * @param[in] subject a pointer to the new subject (or NULL)
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_subject           (pn_message_t *msg, const char *subject);

/**
 * Get the reply_to for a message.
 *
 * This operation will return NULL if no reply_to has been set or if
 * the reply_to has been set to NULL. The pointer returned by this
 * operation is valid until any one of the following operations occur:
 *
 *  - ::pn_message_free()
 *  - ::pn_message_clear()
 *  - ::pn_message_set_reply_to()
 *
 * @param[in] msg a message object
 * @return a pointer to the reply_to of the message (or NULL)
 */
PN_EXTERN const char *   pn_message_get_reply_to          (pn_message_t *msg);

/**
 * Set the reply_to for a message.
 *
 * The supplied reply_to pointer must either be NULL or reference a NUL
 * terminated string. When the pointer is NULL, the reply_to is set to
 * NULL. When the pointer is non NULL, the contents are copied into
 * the message.
 *
 * @param[in] msg a message object
 * @param[in] reply_to a pointer to the new reply_to (or NULL)
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_reply_to          (pn_message_t *msg, const char *reply_to);

/**
 * Get/set the correlation id for a message.
 *
 * A correlation id can be an a string, an unsigned long, a uuid or a
 * binary value. This operation returns a pointer to a ::pn_data_t
 * that can be used to access and/or modify the value of the
 * correlation id. The pointer is valid until the message is freed.
 * See ::pn_data_t for details on how to get/set the value.
 *
 * @param[in] msg a message object
 * @return pointer to a ::pn_data_t holding the correlation id
 */
PN_EXTERN pn_data_t *    pn_message_correlation_id        (pn_message_t *msg);

/**
 * Get the correlation id for a message.
 *
 * A correlation id can be an a string, an unsigned long, a uuid or a
 * binary value. This operation returns the value of the id using the
 * ::pn_atom_t descriminated union. See ::pn_atom_t for details on how
 * to access the value.
 *
 * @param[in] msg a message object
 * @return the message id
 */
PN_EXTERN pn_atom_t      pn_message_get_correlation_id    (pn_message_t *msg);

/**
 * Set the correlation id for a message.
 *
 * See ::pn_message_get_correlation_id() for more details on the
 * meaning of the correlation id. Note that only string, unsigned
 * long, uuid, or binary values are permitted.
 *
 * @param[in] msg a message object
 * @param[in] id the new value of the message id
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_correlation_id    (pn_message_t *msg, pn_atom_t id);

/**
 * Get the content_type for a message.
 *
 * This operation will return NULL if no content_type has been set or if
 * the content_type has been set to NULL. The pointer returned by this
 * operation is valid until any one of the following operations occur:
 *
 *  - ::pn_message_free()
 *  - ::pn_message_clear()
 *  - ::pn_message_set_content_type()
 *
 * @param[in] msg a message object
 * @return a pointer to the content_type of the message (or NULL)
 */
PN_EXTERN const char *   pn_message_get_content_type      (pn_message_t *msg);

/**
 * Set the content_type for a message.
 *
 * The supplied content_type pointer must either be NULL or reference a NUL
 * terminated string. When the pointer is NULL, the content_type is set to
 * NULL. When the pointer is non NULL, the contents are copied into
 * the message.
 *
 * @param[in] msg a message object
 * @param[in] type a pointer to the new content_type (or NULL)
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_content_type      (pn_message_t *msg, const char *type);

/**
 * Get the content_encoding for a message.
 *
 * This operation will return NULL if no content_encoding has been set or if
 * the content_encoding has been set to NULL. The pointer returned by this
 * operation is valid until any one of the following operations occur:
 *
 *  - ::pn_message_free()
 *  - ::pn_message_clear()
 *  - ::pn_message_set_content_encoding()
 *
 * @param[in] msg a message object
 * @return a pointer to the content_encoding of the message (or NULL)
 */
PN_EXTERN const char *   pn_message_get_content_encoding  (pn_message_t *msg);

/**
 * Set the content_encoding for a message.
 *
 * The supplied content_encoding pointer must either be NULL or reference a NUL
 * terminated string. When the pointer is NULL, the content_encoding is set to
 * NULL. When the pointer is non NULL, the contents are copied into
 * the message.
 *
 * @param[in] msg a message object
 * @param[in] encoding a pointer to the new content_encoding (or NULL)
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_content_encoding  (pn_message_t *msg, const char *encoding);

/**
 * Get the expiry time for a message.
 *
 * A zero value for the expiry time indicates that the message will
 * never expire. This is the default value.
 *
 * @param[in] msg a message object
 * @return the expiry time for the message
 */
PN_EXTERN pn_timestamp_t pn_message_get_expiry_time       (pn_message_t *msg);

/**
 * Set the expiry time for a message.
 *
 * See ::pn_message_get_expiry_time() for more details.
 *
 * @param[in] msg a message object
 * @param[in] time the new expiry time for the message
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_expiry_time       (pn_message_t *msg, pn_timestamp_t time);

/**
 * Get the creation time for a message.
 *
 * A zero value for the creation time indicates that the creation time
 * has not been set. This is the default value.
 *
 * @param[in] msg a message object
 * @return the creation time for the message
 */
PN_EXTERN pn_timestamp_t pn_message_get_creation_time     (pn_message_t *msg);

/**
 * Set the creation time for a message.
 *
 * See ::pn_message_get_creation_time() for more details.
 *
 * @param[in] msg a message object
 * @param[in] time the new creation time for the message
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_creation_time     (pn_message_t *msg, pn_timestamp_t time);

/**
 * Get the group_id for a message.
 *
 * This operation will return NULL if no group_id has been set or if
 * the group_id has been set to NULL. The pointer returned by this
 * operation is valid until any one of the following operations occur:
 *
 *  - ::pn_message_free()
 *  - ::pn_message_clear()
 *  - ::pn_message_set_group_id()
 *
 * @param[in] msg a message object
 * @return a pointer to the group_id of the message (or NULL)
 */
PN_EXTERN const char *   pn_message_get_group_id          (pn_message_t *msg);

/**
 * Set the group_id for a message.
 *
 * The supplied group_id pointer must either be NULL or reference a NUL
 * terminated string. When the pointer is NULL, the group_id is set to
 * NULL. When the pointer is non NULL, the contents are copied into
 * the message.
 *
 * @param[in] msg a message object
 * @param[in] group_id a pointer to the new group_id (or NULL)
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_group_id          (pn_message_t *msg, const char *group_id);

/**
 * Get the group sequence for a message.
 *
 * The group sequence of a message identifies the relative ordering of
 * messages within a group. The default value for the group sequence
 * of a message is zero.
 *
 * @param[in] msg a message object
 * @return the group sequence for the message
 */
PN_EXTERN pn_sequence_t  pn_message_get_group_sequence    (pn_message_t *msg);

/**
 * Set the group sequence for a message.
 *
 * See ::pn_message_get_group_sequence() for details on what the group
 * sequence means.
 *
 * @param[in] msg a message object
 * @param[in] n the new group sequence for the message
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_group_sequence    (pn_message_t *msg, pn_sequence_t n);

/**
 * Get the reply_to_group_id for a message.
 *
 * This operation will return NULL if no reply_to_group_id has been set or if
 * the reply_to_group_id has been set to NULL. The pointer returned by this
 * operation is valid until any one of the following operations occur:
 *
 *  - ::pn_message_free()
 *  - ::pn_message_clear()
 *  - ::pn_message_set_reply_to_group_id()
 *
 * @param[in] msg a message object
 * @return a pointer to the reply_to_group_id of the message (or NULL)
 */
PN_EXTERN const char *   pn_message_get_reply_to_group_id (pn_message_t *msg);

/**
 * Set the reply_to_group_id for a message.
 *
 * The supplied reply_to_group_id pointer must either be NULL or reference a NUL
 * terminated string. When the pointer is NULL, the reply_to_group_id is set to
 * NULL. When the pointer is non NULL, the contents are copied into
 * the message.
 *
 * @param[in] msg a message object
 * @param[in] reply_to_group_id a pointer to the new reply_to_group_id (or NULL)
 * @return zero on success or an error code on failure
 */
PN_EXTERN int            pn_message_set_reply_to_group_id (pn_message_t *msg, const char *reply_to_group_id);

/**
 * @deprecated
 */
PN_EXTERN pn_format_t pn_message_get_format(pn_message_t *message);

/**
 * @deprecated
 */
PN_EXTERN int pn_message_set_format(pn_message_t *message, pn_format_t format);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_load(pn_message_t *message, const char *data, size_t size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_load_data(pn_message_t *message, const char *data, size_t size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_load_text(pn_message_t *message, const char *data, size_t size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_load_amqp(pn_message_t *message, const char *data, size_t size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_load_json(pn_message_t *message, const char *data, size_t size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_save(pn_message_t *message, char *data, size_t *size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_save_data(pn_message_t *message, char *data, size_t *size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_save_text(pn_message_t *message, char *data, size_t *size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_save_amqp(pn_message_t *message, char *data, size_t *size);

/**
 * @deprecated Use ::pn_message_body() instead.
 */
PN_EXTERN int pn_message_save_json(pn_message_t *message, char *data, size_t *size);

/**
 * Get/set the delivery instructions for a message.
 *
 * This operation returns a pointer to a ::pn_data_t representing the
 * content of the delivery instructions section of a message. The
 * pointer is valid until the message is freed and may be used to both
 * access and modify the content of the delivery instructions section
 * of a message.
 *
 * The ::pn_data_t must either be empty or consist of a symbol keyed
 * map in order to be considered valid delivery instructions.
 *
 * @param[in] msg a message object
 * @return a pointer to the delivery instructions
 */
PN_EXTERN pn_data_t *pn_message_instructions(pn_message_t *msg);

/**
 * Get/set the annotations for a message.
 *
 * This operation returns a pointer to a ::pn_data_t representing the
 * content of the annotations section of a message. The pointer is
 * valid until the message is freed and may be used to both access and
 * modify the content of the annotations section of a message.
 *
 * The ::pn_data_t must either be empty or consist of a symbol keyed
 * map in order to be considered valid message annotations.
 *
 * @param[in] msg a message object
 * @return a pointer to the message annotations
 */
PN_EXTERN pn_data_t *pn_message_annotations(pn_message_t *msg);

/**
 * Get/set the properties for a message.
 *
 * This operation returns a pointer to a ::pn_data_t representing the
 * content of the properties section of a message. The pointer is
 * valid until the message is freed and may be used to both access and
 * modify the content of the properties section of a message.
 *
 * The ::pn_data_t must either be empty or consist of a string keyed
 * map in order to be considered valid message properties.
 *
 * @param[in] msg a message object
 * @return a pointer to the message properties
 */
PN_EXTERN pn_data_t *pn_message_properties(pn_message_t *msg);

/**
 * Get/set the body of a message.
 *
 * This operation returns a pointer to a ::pn_data_t representing the
 * body of a message. The pointer is valid until the message is freed
 * and may be used to both access and modify the content of the
 * message body.
 *
 * @param[in] msg a message object
 * @return a pointer to the message body
 */
PN_EXTERN pn_data_t *pn_message_body(pn_message_t *msg);

/**
 * Decode/load message content from AMQP formatted binary data.
 *
 * Upon invoking this operation, any existing message content will be
 * cleared and replaced with the content from the provided binary
 * data.
 *
 * @param[in] msg a message object
 * @param[in] bytes the start of the encoded AMQP data
 * @param[in] size the size of the encoded AMQP data
 * @return zero on success or an error code on failure
 */
PN_EXTERN int pn_message_decode(pn_message_t *msg, const char *bytes, size_t size);

/**
 * Encode/save message content as AMQP formatted binary data.
 *
 * If the buffer space provided is insufficient to store the content
 * held in the message, the operation will fail and return a
 * ::PN_OVERFLOW error code.
 *
 * @param[in] msg a message object
 * @param[in] bytes the start of empty buffer space
 * @param[in] size the amount of empty buffer space
 * @param[out] size the amount of data written
 * @return zero on success or an error code on failure
 */
PN_EXTERN int pn_message_encode(pn_message_t *msg, char *bytes, size_t *size);

/**
 * @deprecated
 */
PN_EXTERN ssize_t pn_message_data(char *dst, size_t available, const char *src, size_t size);

/** @}
 */

#ifdef __cplusplus
}
#endif

#endif /* message.h */
