/*
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
 */
typedef unsigned int size_t;
typedef signed int ssize_t;
typedef unsigned char uint8_t;
typedef signed char int8_t;
typedef unsigned short uint16_t;
typedef signed short int16_t;
typedef unsigned long int uint32_t;
typedef long int int32_t;
typedef unsigned long long int uint64_t;
typedef long long int int64_t;
typedef unsigned long int uintptr_t;

/* Parse these interface header files to generate APIs for script languages */

%include "proton/import_export.h"

%ignore _PROTON_VERSION_H;
%include "proton/version.h"

/* We cannot safely just wrap pn_bytes_t but each language binding must have a typemap for it - presumably to a string type */
%ignore pn_bytes_t;

/* There is no need to wrap pn_class_t aa it is an internal implementation detail and cannot be used outside the library */
%ignore pn_class_t;

/* Ignore C APIs related to pn_atom_t - they can all be achieved with pn_data_t */
%ignore pn_atom_t;
%ignore pn_atom_t_u; /* Seem to need this even though its nested in pn_atom_t */
%ignore pn_data_get_atom;
%ignore pn_data_put_atom;

%ignore pn_delivery_tag_t;
%ignore pn_decimal128_t;
%ignore pn_uuid_t;

%include "proton/types.h"
%ignore pn_string_vformat;
%ignore pn_string_vaddf;
%immutable PN_OBJECT;
%immutable PN_VOID;
%immutable PN_WEAKREF;
%include "proton/object.h"

%ignore pn_error_format;
%ignore pn_error_vformat;


/* checks that ensure only allowed values are supplied or returned */
%aggregate_check(int, check_error,
                 PN_EOS, PN_ERR, PN_OVERFLOW, PN_UNDERFLOW,
                 PN_STATE_ERR, PN_ARG_ERR, PN_TIMEOUT);

%aggregate_check(int, check_state,
                 PN_LOCAL_UNINIT, PN_LOCAL_ACTIVE, PN_LOCAL_CLOSED,
                 PN_REMOTE_UNINIT, PN_REMOTE_ACTIVE, PN_REMOTE_CLOSED);

%aggregate_check(int, check_disposition, 0,
                 PN_RECEIVED, PN_ACCEPTED, PN_REJECTED,
                 PN_RELEASED, PN_MODIFIED);

%aggregate_check(int, check_trace,
                 PN_TRACE_OFF, PN_TRACE_RAW, PN_TRACE_FRM, PN_TRACE_DRV);

%aggregate_check(int, check_format,
                 PN_DATA, PN_TEXT, PN_AMQP, PN_JSON);

%aggregate_check(int, check_sasl_outcome,
                 PN_SASL_NONE, PN_SASL_OK, PN_SASL_AUTH,
                 PN_SASL_SYS, PN_SASL_PERM, PN_SASL_TEMP, PN_SASL_SKIPPED);

%aggregate_check(int, check_sasl_state,
                 PN_SASL_CONF, PN_SASL_IDLE, PN_SASL_STEP,
                 PN_SASL_PASS, PN_SASL_FAIL);


%contract pn_code(int code)
{
 require:
  check_error(code);
}

%contract pn_error()
{
 ensure:
  pn_error != NULL;
}

%contract pn_error_free(pn_error_t *error)
{
 require:
  error != NULL;
}

%contract pn_error_clear(pn_error_t *error)
{
 require:
  error != NULL;
}

%contract pn_error_set(pn_error_t *error, int code, const char *text)
{
 require:
  error != NULL;
}

%contract pn_error_vformat(pn_error_t *error, int code, const char *fmt, va_list ap)
{
 require:
  error != NULL;
}

%contract pn_error_format(pn_error_t *error, int code, const char *fmt, ...)
{
 require:
  error != NULL;
}

%contract pn_error_code(pn_error_t *error)
{
 require:
  error != NULL;
}

%contract pn_error_text(pn_error_t *error)
{
 require:
  error != NULL;
}

%include "proton/error.h"

%contract pn_connection(void)
{
 ensure:
  pn_connection != NULL;
}

%contract pn_connection_state(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_connection_error(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_connection_get_container(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_connection_set_container(pn_connection_t *connection, const char *container)
{
 require:
  connection != NULL;
}

%contract pn_connection_get_hostname(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_connection_set_hostname(pn_connection_t *connection, const char *hostname)
{
 require:
  connection != NULL;
}

%contract pn_connection_remote_container(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_connection_remote_hostname(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_work_head(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_work_next(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_session(pn_connection_t *connection)
{
 require:
  connection != NULL;
 ensure:
  pn_session != NULL;
}

%contract pn_transport(pn_connection_t *connection)
{
 require:
  connection != NULL;
 ensure:
  pn_transport != NULL;
}

%contract pn_session_head(pn_connection_t *connection, pn_state_t state)
{
 require:
  connection != NULL;
}

%contract pn_session_next(pn_session_t *session, pn_state_t state)
{
 require:
  session != NULL;
}

%contract pn_link_head(pn_connection_t *connection, pn_state_t state)
{
 require:
  connection != NULL;
}

%contract pn_link_next(pn_link_t *link, pn_state_t state)
{
 require:
  link != NULL;
}

%contract pn_connection_open(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_connection_close(pn_connection_t *connection)
{
 require:
 connection != NULL;
}

%contract pn_connection_free(pn_connection_t *connection)
{
 require:
  connection != NULL;
}

%contract pn_transport_error(pn_transport_t *transport)
{
 require:
  transport != NULL;
}

%contract pn_transport_input(pn_transport_t *transport, char *bytes, size_t available)
{
 require:
  transport != NULL;
}

%contract pn_transport_output(pn_transport_t *transport, char *bytes, size_t size)
{
 require:
  transport != NULL;
}

#%contract pn_transport_tick(pn_transport_t *transport, pn_timestamp_t now)
#{
#  # this method currently always returns 0
#}

%contract pn_transport_trace(pn_transport_t *transport, pn_trace_t trace)
{
 require:
  transport != NULL;
}

%contract pn_transport_free(pn_transport_t *transport)
{
 require:
  transport != NULL;
}

%contract pn_session_state(pn_session_t *session)
{
 require:
  session != NULL;
}

%contract pn_session_error(pn_session_t *session)
{
 require:
  session != NULL;
}

%contract pn_sender(pn_session_t *session, const char *name)
{
 require:
  session != NULL;
 ensure:
  pn_sender != NULL;
}

%contract pn_receiver(pn_session_t *session, const char *name)
{
 require:
  session != NULL;
 ensure:
  pn_receiver != NULL;
}

%contract pn_session_connection(pn_session_t *session)
{
 require:
  session != NULL;
 ensure:
  pn_session_connection != NULL;
}

%contract pn_session_open(pn_session_t *session)
{
 require:
  session != NULL;
}

%contract pn_session_close(pn_session_t *session)
{
 require:
  session != NULL;
}

%contract pn_session_free(pn_session_t *session)
{
 require:
  session != NULL;
}

%contract pn_link_name(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_is_sender(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_is_receiver(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_state(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_error(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_session(pn_link_t *link)
{
 require:
  link != NULL;
 ensure:
  pn_link_session != NULL;
}

%contract pn_link_get_target(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_get_source(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_set_source(pn_link_t *link, const char *source)
{
 require:
  link != NULL;
}

%contract pn_link_set_target(pn_link_t *link, const char *target)
{
 require:
  link != NULL;
}

%contract pn_link_remote_source(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_remote_target(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_delivery(pn_link_t *link, pn_delivery_tag_t tag)
{
 require:
  link != NULL;
 ensure:
  pn_delivery != NULL;
}

%contract pn_link_current(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_advance(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_credit(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_queued(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_link_unsettled(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_unsettled_head(pn_link_t *link)
{
 require:
  link != NULL;
}

%contract pn_unsettled_next(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_link_open(pn_link_t *sender)
{
 require:
  sender != NULL;
}

%contract pn_link_close(pn_link_t *sender)
{
 require:
  sender != NULL;
}

%contract pn_link_free(pn_link_t *sender)
{
 require:
  sender != NULL;
}

%contract pn_link_send(pn_link_t *sender, const char *bytes, size_t n)
{
 require:
  sender != NULL;
}

%contract pn_link_drained(pn_link_t *sender)
{
 require:
  sender != NULL;
}

%contract pn_link_flow(pn_link_t *receiver, int credit)
{
 require:
  receiver != NULL;
}

%contract pn_link_drain(pn_link_t *receiver, int credit)
{
 require:
  receiver != NULL;
}

%contract pn_link_recv(pn_link_t *receiver, char *bytes, size_t n)
{
 require:
  receiver != NULL;
}

%contract pn_delivery_tag(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_link(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_local_state(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_remote_state(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_settled(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_pending(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_writable(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_readable(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_updated(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_clear(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_update(pn_delivery_t *delivery, pn_disposition_t disposition)
{
 require:
  delivery != NULL;
  check_disposition(disposition);
}

%contract pn_delivery_settle(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%contract pn_delivery_dump(pn_delivery_t *delivery)
{
 require:
  delivery != NULL;
}

%include "proton/condition.h"
%include "proton/connection.h"
%include "proton/session.h"
%include "proton/link.h"
%include "proton/terminus.h"
%include "proton/delivery.h"
%include "proton/disposition.h"
%include "proton/transport.h"
%include "proton/event.h"

%contract pn_message_free(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_clear(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_errno(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_error(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_is_durable(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_durable(pn_message_t *msg, bool durable)
{
 require:
  msg != NULL;
}

%contract pn_message_get_priority(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_priority(pn_message_t *msg, uint8_t priority)
{
 require:
  msg != NULL;
}

%contract pn_message_get_ttl(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_ttl(pn_message_t *msg, pn_millis_t ttl)
{
 require:
  msg != NULL;
}

%contract pn_message_is_first_acquirer(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_first_acquirer(pn_message_t *msg, bool first)
{
 require:
  msg != NULL;
}

%contract pn_message_get_delivery_count(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_delivery_count(pn_message_t *msg, uint32_t count)
{
 require:
  msg != NULL;
}

%contract pn_message_get_id(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_id(pn_message_t *msg, pn_atom_t id)
{
 require:
  msg != NULL;
}

%contract pn_message_get_user_id(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_user_id(pn_message_t *msg, pn_bytes_t user_id)
{
 require:
  msg != NULL;
}

%contract pn_message_get_address(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_address(pn_message_t *msg, const char *address)
{
 require:
  msg != NULL;
}

%contract pn_message_get_subject(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_subject(pn_message_t *msg, const char *subject)
{
 require:
  msg != NULL;
}

%contract pn_message_get_reply_to(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_reply_to(pn_message_t *msg, const char *reply_to)
{
 require:
  msg != NULL;
}

%contract pn_message_get_correlation_id(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_correlation_id(pn_message_t *msg, pn_atom_t atom)
{
 require:
  msg != NULL;
}

%contract pn_message_get_content_type(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_content_type(pn_message_t *msg, const char *type)
{
 require:
  msg != NULL;
}

%contract pn_message_get_content_encoding(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_content_encoding(pn_message_t *msg, const char *encoding)
{
 require:
  msg != NULL;
}

%contract pn_message_get_expiry_time(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_expiry_time(pn_message_t *msg, pn_timestamp_t time)
{
 require:
  msg != NULL;
}

%contract pn_message_get_creation_time(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_creation_time(pn_message_t *msg, pn_timestamp_t time)
{
 require:
  msg != NULL;
}

%contract pn_message_get_group_id(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_group_id(pn_message_t *msg, const char *group_id)
{
 require:
  msg != NULL;
}

%contract pn_message_get_group_sequence(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_group_sequence(pn_message_t *msg, pn_sequence_t n)
{
 require:
  msg != NULL;
}

%contract pn_message_get_reply_to_group_id(pn_message_t *msg)
{
 require:
  msg != NULL;
}

%contract pn_message_set_reply_to_group_id(pn_message_t *msg, const char *reply_to_group_id)
{
 require:
  msg != NULL;
}

%contract pn_message_get_format(pn_message_t *message)
{
 require:
  message != NULL;
 ensure:
  check_format(pn_message_get_format);
}

%contract pn_message_set_format(pn_message_t *message, pn_format_t format)
{
 require:
  message != NULL;
  check_format(format);
}

%contract pn_message_load(pn_message_t *message, const char *data, size_t size)
{
 require:
  message != NULL;
  size >= 0;
}

%contract pn_message_load_data(pn_message_t *message, const char *data, size_t size)
{
 require:
  message != NULL;
  size >= 0;
}

%contract pn_message_load_text(pn_message_t *message, const char *data, size_t size)
{
 require:
  message != NULL;
  size >= 0;
}

%contract pn_message_load_amqp(pn_message_t *message, const char *data, size_t size)
{
 require:
  message != NULL;
  size >= 0;
}

%contract pn_message_load_json(pn_message_t *message, const char *data, size_t size)
{
 require:
  message != NULL;
  size >= 0;
}

%contract pn_message_save(pn_message_t *message, char *data, size_t *size)
{
 require:
  message != NULL;
  *size >= 0;
}

%contract pn_message_save_data(pn_message_t *message, char *data, size_t *size)
{
 require:
  message != NULL;
  *size >= 0;
}

%contract pn_message_save_text(pn_message_t *message, char *data, size_t *size)
{
 require:
  message != NULL;
  *size >= 0;
}

%contract pn_message_save_amqp(pn_message_t *message, char *data, size_t *size)
{
 require:
  message != NULL;
  *size >= 0;
}

%contract pn_message_save_json(pn_message_t *message, char *data, size_t *size)
{
 require:
  message != NULL;
  *size >= 0;
}

%contract pn_message_decode(pn_message_t *msg, const char *bytes, size_t size)
{
 require:
  msg != NULL;
  size >= 0;
}

%contract pn_message_encode(pn_message_t *msg, char *bytes, size_t *size)
{
 require:
  msg != NULL;
  *size >= 0;
}

%contract pn_message_data(char *dst, size_t available, const char *src, size_t size)
{
 ensure:
  pn_message_data >= 0;
}

%include "proton/message.h"

%contract pn_sasl()
{
 ensure:
  pn_sasl != NULL;
}

%contract pn_sasl_state(pn_sasl_t *sasl)
{
 require:
  sasl != NULL;
 ensure:
  check_sasl_state(pn_sasl_state);
}

%contract pn_sasl_mechanisms(pn_sasl_t *sasl, const char *mechanisms)
{
 require:
  sasl != NULL;
}

%contract pn_sasl_remote_mechanisms(pn_sasl_t *sasl)
{
 require:
  sasl != NULL;
}

%contract pn_sasl_client(pn_sasl_t *sasl)
{
 require:
  sasl != NULL;
}

%contract pn_sasl_server(pn_sasl_t *sasl)
{
 require:
  sasl != NULL;
}

%contract pn_sasl_allow_skip(pn_sasl_t *sasl, bool allow)
{
 require:
  sasl != NULL;
}

%contract pn_sasl_plain(pn_sasl_t *sasl, const char *username, const char *password)
{
 require:
  sasl != NULL;
}

%contract pn_sasl_pending(pn_sasl_t *sasl)
{
 require:
  sasl != NULL;
}

%contract pn_sasl_recv(pn_sasl_t *sasl, char *bytes, size_t size)
{
 require:
  sasl != NULL;
  bytes != NULL;
  size > 0;
}

%contract pn_sasl_send(pn_sasl_t *sasl, const char *bytes, size_t size)
{
 require:
  sasl != NULL;
  bytes != NULL;
  size > 0;
}

%contract pn_sasl_done(pn_sasl_t *sasl, pn_sasl_outcome_t outcome)
{
 require:
  sasl != NULL;
  check_sasl_outcome(outcome);
}

%contract pn_sasl_outcome(pn_sasl_t *sasl)
{
 require:
  sasl != NULL;
 ensure:
  check_sasl_outcome(pn_sasl_outcome);
}

%include "proton/sasl.h"

%contract pn_driver(void)
{
 ensure:
  pn_driver != NULL;
}

%contract pn_driver_trace(pn_driver_t *driver, pn_trace_t trace)
{
 require:
  driver != NULL;
  check_trace(trace);
}

%contract pn_driver_wakeup(pn_driver_t *driver)
{
 require:
  driver != NULL;
}

%contract pn_driver_wait(pn_driver_t *driver, int timeout)
{
 require:
  driver != NULL;
  timeout >= -1;
}

/** Get the next listener with pending data in the driver.
 *
 * @param[in] driver the driver
 * @return NULL if no active listener available
 */
%contract pn_driver_listener(pn_driver_t *driver)
{
 require:
  driver != NULL;
}

%contract pn_driver_connector(pn_driver_t *driver)
{
 require:
  driver != NULL;
}

%contract pn_driver_free(pn_driver_t *driver)
{
 require:
  driver != NULL;
}

%contract pn_listener(pn_driver_t *driver, const char *host,
                      const char *port, void* context)
{
 require:
  driver != NULL;
  host != NULL;
  port != NULL;
}

%contract pn_listener_fd(pn_driver_t *driver, int fd, void *context)
{
 require:
  driver != NULL;
  fd >= 0;
}

%contract pn_listener_trace(pn_listener_t *listener, pn_trace_t trace)
{
 require:
  listener != NULL;
  check_trace(trace);
}

%contract pn_listener_accept(pn_listener_t *listener)
{
 require:
  listener != NULL;
 ensure:
  pn_listener_accept != NULL;
}

%contract pn_listener_context(pn_listener_t *listener)
{
 require:
  listener != NULL;
}

%contract pn_listener_set_context(pn_listener_t *listener, void *context)
{
 require:
  listener != NULL;
}

%contract pn_listener_close(pn_listener_t *listener)
{
 require:
  listener != NULL;
}

%contract pn_listener_free(pn_listener_t *listener)
{
 require:
  listener != NULL;
}


%contract pn_connector(pn_driver_t *driver, const char *host,
                       const char *port, void* context)
{
 require:
  driver != NULL;
  host != NULL;
  port != NULL;
 ensure:
  pn_connector != NULL;
}

%contract pn_connector_fd(pn_driver_t *driver, int fd, void *context)
{
 require:
  driver != NULL;
  fd >= 0;
 ensure:
  pn_connector_fd != NULL;
}

%contract pn_connector_trace(pn_connector_t *connector, pn_trace_t trace)
{
 require:
  connector != NULL;
  check_trace(trace);
}

%contract pn_connector_process(pn_connector_t *connector)
{
 require:
  connector != NULL;
}

%contract pn_connector_listener(pn_connector_t *connector)
{
 require:
  connector != NULL;
}

%contract pn_connector_sasl(pn_connector_t *connector)
{
 require:
  connector != NULL;
}

%contract pn_connector_connection(pn_connector_t *connector)
{
 require:
  connector != NULL;
}

%contract pn_connector_set_connection(pn_connector_t *ctor, pn_connection_t *connection)
{
 require:
  ctor != NULL;
}

%contract pn_connector_context(pn_connector_t *connector)
{
 require:
  connector != NULL;
}

%contract pn_connector_set_context(pn_connector_t *connector, void *context)
{
 require:
  connector != NULL;
}

%contract pn_connector_close(pn_connector_t *connector)
{
 require:
  connector != NULL;
}

%contract pn_connector_closed(pn_connector_t *connector)
{
 require:
  connector != NULL;
}

%contract pn_connector_free(pn_connector_t *connector)
{
 require:
  connector != NULL;
}


%include "proton/driver.h"
%include "proton/driver_extras.h"

%contract pn_messenger(const char *name)
{
 ensure:
  pn_message != NULL;
}

%contract pn_messenger_name(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
 ensure:
  pn_messenger_name != NULL;
}

%contract pn_messenger_set_timeout(pn_messenger_t *messenger, int timeout)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_get_timeout(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_free(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_errno(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_error(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_start(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_stop(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_subscribe(pn_messenger_t *messenger, const char *source)
{
 require:
  messenger != NULL;
  source != NULL;
}

%contract pn_messenger_put(pn_messenger_t *messenger, pn_message_t *msg)
{
 require:
  messenger != NULL;
  msg != NULL;
}

%contract pn_messenger_send(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_recv(pn_messenger_t *messenger, int n)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_get(pn_messenger_t *messenger, pn_message_t *msg)
{
 require:
  messenger != NULL;
}

%contract pn_messenger_outgoing(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
 ensure:
  pn_messenger_outgoing >= 0;
}

%contract pn_messenger_incoming(pn_messenger_t *messenger)
{
 require:
  messenger != NULL;
 ensure:
  pn_messenger_incoming >= 0;
}


%include "proton/messenger.h"

%include "proton/io.h"

%include "proton/selectable.h"

%include "proton/ssl.h"

%ignore pn_decode_atoms;
%ignore pn_encode_atoms;
%ignore pn_decode_one;

%ignore pn_print_atom;
%ignore pn_type_str;
%ignore pn_print_atoms;
%ignore pn_format_atoms;
%ignore pn_format_atom;

%ignore pn_fill_atoms;
%ignore pn_vfill_atoms;
%ignore pn_ifill_atoms;
%ignore pn_vifill_atoms;
%ignore pn_scan_atoms;
%ignore pn_vscan_atoms;
%ignore pn_data_vfill;
%ignore pn_data_vscan;

%include "proton/codec.h"

%inline %{
  pn_connection_t *pn_cast_pn_connection(void *x) { return (pn_connection_t *) x; }
  pn_session_t *pn_cast_pn_session(void *x) { return (pn_session_t *) x; }
  pn_link_t *pn_cast_pn_link(void *x) { return (pn_link_t *) x; }
  pn_delivery_t *pn_cast_pn_delivery(void *x) { return (pn_delivery_t *) x; }
  pn_transport_t *pn_cast_pn_transport(void *x) { return (pn_transport_t *) x; }
%}

%include "proton/url.h"

