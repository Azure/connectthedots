#ifndef __COMMON_H
#define __COMMON_H

#ifdef _WIN32
#define SNPRINTF _snprintf
#include <BaseTsd.h>
typedef SSIZE_T ssize_t;
#else
#define SNPRINTF snprintf
#endif

#ifndef SERVICEBUS_DOMAIN
#define SERVICEBUS_DOMAIN	"servicebus.int7.windows-int.net"
#endif

#include "proton/messenger.h"
#include "proton/error.h"
#ifndef PN_VERSION_MAJOR
#include "proton/version.h"
#endif

#ifdef _WIN32
#include <rpc.h>
#else
#include <uuid/uuid.h>
#endif


void protonError(int err, char *step, pn_messenger_t *messenger)
{
	const char *errMsg = NULL;

	if (0 == err)
	{
		return;
	}
	printf("ERROR: PROTON API: %s Errno: %d (%s)\n", step, err, pn_code(err));

#if (PN_VERSION_MINOR == 4)
	/*
	** Proton-C 0.4 returns an error message directly
	*/
	errMsg = pn_messenger_error(messenger);
	if (NULL == errMsg)
	{
		errMsg = "pn_messenger_error() returned NULL";
	}
#else
	/*
	** Proton-C 0.5 and later returns an error structure which
	** you retrieve the error message from as a separate step.
	*/
	{
		pn_error_t *errInfo = pn_messenger_error(messenger);
		errMsg = pn_error_text(errInfo);
	}
#endif

	printf("ERROR: PROTON Msg: %s\n", (NULL == errMsg) ? "NULL" : errMsg);
}


void generateUuid(pn_uuid_t *pGenerated)
{
#ifdef _WIN32
	UUID u;
	UuidCreate(&u);
	/* assuming little-endian */
	pGenerated->bytes[0] = (char)((u.Data1 >> 24) & 0x000000FF);
	pGenerated->bytes[1] = (char)((u.Data1 >> 16) & 0x000000FF);
	pGenerated->bytes[2] = (char)((u.Data1 >> 8) & 0x000000FF);
	pGenerated->bytes[3] = (char)((u.Data1 >> 0) & 0x000000FF);
	pGenerated->bytes[4] = (char)((u.Data2 >> 8) & 0x000000FF);
	pGenerated->bytes[5] = (char)((u.Data2 >> 0) & 0x000000FF);
	pGenerated->bytes[6] = (char)((u.Data3 >> 8) & 0x000000FF);
	pGenerated->bytes[7] = (char)((u.Data3 >> 0) & 0x000000FF);
	memcpy(pGenerated->bytes + 8, u.Data4, 8);
#else
	/* For Linux and libuuid, uuid_t is bitwise the same as pn_uuid_t */
	uuid_t u;
	uuid_generate(u);
	memcpy(pGenerated, u, sizeof(u));
#endif
}


void outputUuid(pn_uuid_t *pUuid)
{
	printf("%02x%02x%02x%02x-%02x%02x-%02x%02x-"
		"%02x%02x%02x%02x%02x%02x%02x%02x\n",
		((int)pUuid->bytes[0] & 0x00FF),
		((int)pUuid->bytes[1] & 0x00FF),
		((int)pUuid->bytes[2] & 0x00FF),
		((int)pUuid->bytes[3] & 0x00FF),
		((int)pUuid->bytes[4] & 0x00FF),
		((int)pUuid->bytes[5] & 0x00FF),
		((int)pUuid->bytes[6] & 0x00FF),
		((int)pUuid->bytes[7] & 0x00FF),
		((int)pUuid->bytes[8] & 0x00FF),
		((int)pUuid->bytes[9] & 0x00FF),
		((int)pUuid->bytes[10] & 0x00FF),
		((int)pUuid->bytes[11] & 0x00FF),
		((int)pUuid->bytes[12] & 0x00FF),
		((int)pUuid->bytes[13] & 0x00FF),
		((int)pUuid->bytes[14] & 0x00FF),
		((int)pUuid->bytes[15] & 0x00FF)
		);
}


char *urlEncodeKey(const char *key)
{
	char *retval = (char *)malloc(512); // overkill
	char *outKey = retval;
	const char *inKey = key;
	while (*inKey != '\0')
	{
		// Base64 only uses three chars that need escaping in URLs
		switch (*inKey)
		{
		case '/':
			*outKey++ = '%';
			*outKey++ = '2';
			*outKey = 'F';
			break;

		case '+':
			*outKey++ = '%';
			*outKey++ = '2';
			*outKey = 'B';
			break;

		case '=':
			*outKey++ = '%';
			*outKey++ = '3';
			*outKey = 'D';
			break;

		default:
			*outKey = *inKey;
			break;

		}
		inKey++;
		outKey++;
	}
	*outKey = '\0';
	return retval;
}


#endif /* __COMMON_H */
