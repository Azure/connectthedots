#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <time.h> 

#include "common.h"

#include "proton/message.h"
#include "proton/messenger.h"
#include "proton/error.h"
#ifndef PN_VERSION_MAJOR
#include "proton/version.h"
#endif

pn_uuid_t deviceId;
bool deviceIdInitialized = false;

void checkTracking(pn_messenger_t *messenger, pn_tracker_t tracker)
{
	pn_status_t status = PN_STATUS_UNKNOWN;
	bool keepTrying = true;

#if (PN_VERSION_MINOR == 4) || !defined(USE_BLOCKING_SEND)
	/*
	** Version 0.4 does not support blocking sends. The only way to find
	** the final status of a send is to keep polling until it changes
	** to a final state (ACCEPTED/REJECTED) or you give up and assume
	** it failed. For later versions, you can set the blocking mode and
	** the send call will not return until it has a final status. Hence,
	** the retry loop is required only for version 0.4 OR if you are
	** not using the blocking mode for send.
	*/
	int retry;
	for (retry = 0; (retry < 10) && keepTrying; retry++)
	{
#endif
		status = pn_messenger_status(messenger, tracker);

		switch (status)
		{
		case PN_STATUS_UNKNOWN:
			printf("Message status PN_STATUS_UNKNOWN\n");
			break;

		case PN_STATUS_PENDING:
			printf("Message status PN_STATUS_PENDING\n");
			break;

		case PN_STATUS_ACCEPTED:
			printf("Message status PN_STATUS_ACCEPTED\n");
			keepTrying = false; /* definitely final state */
			break;

		case PN_STATUS_REJECTED:
			printf("Message status PN_STATUS_REJECTED\n");
			keepTrying = false; /* definitely final state */
			break;

#if (PN_VERSION_MINOR > 4)
			/*
			** New status added in 0.5
			*/
		case PN_STATUS_MODIFIED:
			printf("Message status PN_STATUS_MODIFIED\n");
			break;
#endif

#if (PN_VERSION_MINOR > 5)
			/*
			** New statuses added in 0.6
			*/
		case PN_STATUS_RELEASED:
			printf("Message status PN_STATUS_RELEASED\n");
			keepTrying = false; /* definitely final state */
			break;

		case PN_STATUS_ABORTED:
			printf("Message status PN_STATUS_ABORTED\n");
			keepTrying = false; /* definitely final state */
			break;

		case PN_STATUS_SETTLED:
			printf("Message status PN_STATUS_SETTLED\n");
			keepTrying = false; /* definitely final state */
			break;
#endif

		default:
			printf("Message status UNRECOGNIZED (%d)\n", (int)status);
			break;
		}

#if (PN_VERSION_MINOR == 4) || !defined(USE_BLOCKING_SEND)
		/*
		** Again, only need the loop when not using blocking sends.
		** See the big comment at the top.
		*/
		if ((status != PN_STATUS_ACCEPTED) && keepTrying)
		{
			printf("Sleeping 2 sec to give broker time to ack message\n");
			Sleep(2);
		}
	}
#endif

	printf("Final send status is: ");
	if (PN_STATUS_ACCEPTED == status)
	{
		printf("successful!\n");
	}
	else if (PN_STATUS_REJECTED == status)
	{
		printf("rejected by the broker\n");
	}
	/*
	** For version 0.4, if you keep getting status PENDING, it
	** is possible that the broker is just being slow, or it is possible
	** that the message never even got onto the wire. At some point you
	** have to give up and declare that send failed. It's up to you to
	** determine how long to wait and how many retries are appropriate
	** for your application and environment.

	** For version 0.5, with blocking mode set, a final status of
	** PENDING means something went seriously wrong: the connection is
	** dead, or the broker never responded, for example. This can happen
	** if the destination entity doesn't exist, for example.
	*/
	else if (PN_STATUS_PENDING == status)
	{
		printf("Giving up, assuming send failed\n");
	}
#if (PN_VERSION_MINOR > 5)
	/*
	** For version 0.6, the new status ABORTED was introduced, which
	** means the message never made it onto the wire.
	*/
	else if (PN_STATUS_ABORTED == status)
	{

		printf("failed, never sent on network\n");
	}
#endif
	else
	{
		printf("unclear\n");
	}

	printf("CALL pn_messenger_settle... ");
	int err = pn_messenger_settle(messenger, tracker, PN_CUMULATIVE);
	printf("RETURNED %d\n", err);
	if (err != 0)
	{
		protonError(err, "pn_messenger_settle", messenger);
	}
}

int sender(char *sbnamespace, char *entity, char *issuerName, char *issuerKey, char* sbDomain, char* deviceDisplayName, char* subject, char* msgtext, pn_timestamp_t utcTime)
{
	char address[500];

	// For Proton-C versions 0.4-0.6, the key MUST NOT be URL-encoded.
	// For Proton-C versions 0.7+, the key MUST be URL-encoded.
#if (PN_VERSION_MINOR >= 7)
	char *key = issuerKey;
#else
	char *key = issuerKey;
#endif

	_snprintf_s(address, sizeof(address),
		"amqps://%s:%s@%s.%s/%s",
		issuerName, key, sbnamespace, sbDomain, entity);

	printf("Sending messages to %s\n", address);

	pn_messenger_t *messenger = pn_messenger(NULL);

	printf("CALL pn_messenger_set_outgoing_window... ");
	/*
	** 5 is an arbitrary number here. It is not really necessary
	** with blocking send, but if you are not using blocking send
	** it determines how many outgoing messages you can track the
	** status of.
	*/
	int err = pn_messenger_set_outgoing_window(messenger, 5);
	printf("RETURNED %d\n", err);
	if (err != 0)
	{
		protonError(err, "pn_messenger_set_outgoing_window", messenger);
		return -1;
	}

#if (PN_VERSION_MINOR > 4) && defined(USE_BLOCKING_SEND)
	printf("CALL pn_messenger_set_blocking... ");
	err = pn_messenger_set_blocking(messenger, true);
	printf("RETURNED %d\n", err);
	if (err != 0)
	{
		protonError(err, "pn_messenger_set_blocking", messenger);
		return -1;
	}
#endif

	printf("CALL pn_messenger_start... ");
	err = pn_messenger_start(messenger);
	printf("RETURNED %d\n", err);
	if (err != 0)
	{
		protonError(err, "pn_messenger_start", messenger);
		return -1;
	}

	if (!deviceIdInitialized)
		generateUuid(&deviceId);

	pn_uuid_t id;
	generateUuid(&id);
	pn_message_t * message;
	pn_data_t * body;
	message = pn_message();

	pn_data_t *properties = pn_message_properties(message);

	pn_data_t *annotations = pn_message_annotations(message);

	pn_data_put_map(annotations);
	pn_data_enter(annotations);

	pn_data_put_symbol(annotations, pn_bytes(strlen("x-opt-partition-key"), "x-opt-partition-key"));
	pn_data_put_uuid(annotations, deviceId);

	pn_data_exit(annotations);

	pn_data_put_map(properties);
	pn_data_enter(properties);

	pn_data_put_string(properties, pn_bytes(strlen("Subject"), "Subject"));
	pn_data_put_string(properties, pn_bytes(strlen(subject), subject));

	pn_data_exit(properties);

	pn_message_set_address(message, address);
	pn_message_set_content_type(message, (char*) "text/json");
	pn_message_set_inferred(message, true);


	body = pn_message_body(message);
	pn_data_put_binary(body, pn_bytes(strlen(msgtext), msgtext));

	pn_message_set_subject(message, subject);
	pn_message_set_ttl(message, 86400000);

	//Azure wants a UTC timestamp
	pn_message_set_creation_time(message, utcTime);
	pn_message_set_content_encoding(message, "UTF-8");

	pn_atom_t message_id;
	message_id.type = PN_UUID;
	message_id.u.as_uuid = id;
	pn_message_set_id(message, message_id);

	err = pn_messenger_put(messenger, message);
	printf("RETURNED %d\n", err);
	if (err != 0)
	{
		protonError(err, "pn_messenger_put", messenger);
	}
	printf("CALL pn_messenger_send... ");
#if (PN_VERSION_MINOR == 4)
	/*
	** Proton-C 0.4: pn_messenger_send() always sends all messages which
	** have been queued for send by pn_messenger_put().
	*/
	err = pn_messenger_send(messenger);
#else
	/*
	** Proton-C 0.5 and later: pn_messenger_send() takes a specific number
	** of messages to send, which can also be used to specify blocking or
	** nonblocking. This sample uses -1 which blocks until all outgoing
	** messages have been sent.
	*/
	err = pn_messenger_send(messenger, -1);
#endif
	printf("RETURNED %d\n", err);
	if (0 == err)
	{
		printf("Sent BytesMessage with id\n");
		outputUuid(&id);
	}
	else
	{
		protonError(err, "pn_messenger_send", messenger);
	}

	pn_tracker_t tracker = pn_messenger_outgoing_tracker(messenger);
	checkTracking(messenger, tracker);

	printf("CALL pn_messenger_stop... ");

	err = pn_messenger_stop(messenger);
	printf("RETURNED %d\n", err);
	if (err != 0)
	{
		protonError(err, "pn_messenger_stop", messenger);
	}

	pn_messenger_free(messenger);

	pn_message_free(message);

	return 0;
}