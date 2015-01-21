#ifndef PROTON_IO_H
#define PROTON_IO_H 1

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
#include <sys/types.h>
#include <proton/type_compat.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * A ::pn_socket_t provides an abstract handle to an IO stream.  The
 * pipe version is uni-directional.  The network socket version is
 * bi-directional.  Both are non-blocking.
 *
 * pn_socket_t handles from ::pn_pipe() may only be used with
 * ::pn_read(), ::pn_write(), ::pn_close() and pn_selector_select().
 *
 * pn_socket_t handles from ::pn_listen(), ::pn_accept() and
 * ::pn_connect() must perform further IO using Proton functions.
 * Mixing Proton io.h functions with native IO functions on the same
 * handles will result in undefined behavior.
 *
 * pn_socket_t handles may only be used with a single pn_io_t during
 * their lifetime.
 */
#if defined(_WIN32) && ! defined(__CYGWIN__)
#ifdef _WIN64
typedef unsigned __int64 pn_socket_t;
#else
typedef unsigned int pn_socket_t;
#endif
#define PN_INVALID_SOCKET (pn_socket_t)(~0)
#else
typedef int pn_socket_t;
#define PN_INVALID_SOCKET (-1)
#endif

/**
 * A ::pn_io_t manages IO for a group of pn_socket_t handles.  A
 * pn_io_t object may have zero or one pn_selector_t selectors
 * associated with it (see ::pn_io_selector()).  If one is associated,
 * all the pn_socket_t handles managed by a pn_io_t must use that
 * pn_selector_t instance.
 *
 * The pn_io_t interface is single-threaded. All methods are intended
 * to be used by one thread at a time, except that multiple threads
 * may use:
 *
 *   ::pn_write()
 *   ::pn_send()
 *   ::pn_recv()
 *   ::pn_close()
 *   ::pn_selector_select()
 *
 * provided at most one thread is calling ::pn_selector_select() and
 * the other threads are operating on separate pn_socket_t handles.
 */
typedef struct pn_io_t pn_io_t;

/**
 * A ::pn_selector_t provides a selection mechanism that allows
 * efficient monitoring of a large number of Proton connections and
 * listeners.
 *
 * External (non-Proton) sockets may also be monitored, either solely
 * for event notification (read, write, and timer) or event
 * notification and use with pn_io_t interfaces.
 */
typedef struct pn_selector_t pn_selector_t;

PN_EXTERN pn_io_t *pn_io(void);
PN_EXTERN void pn_io_free(pn_io_t *io);
PN_EXTERN pn_error_t *pn_io_error(pn_io_t *io);
PN_EXTERN pn_socket_t pn_connect(pn_io_t *io, const char *host, const char *port);
PN_EXTERN pn_socket_t pn_listen(pn_io_t *io, const char *host, const char *port);
PN_EXTERN pn_socket_t pn_accept(pn_io_t *io, pn_socket_t socket, char *name, size_t size);
PN_EXTERN void pn_close(pn_io_t *io, pn_socket_t socket);
PN_EXTERN ssize_t pn_send(pn_io_t *io, pn_socket_t socket, const void *buf, size_t size);
PN_EXTERN ssize_t pn_recv(pn_io_t *io, pn_socket_t socket, void *buf, size_t size);
PN_EXTERN int pn_pipe(pn_io_t *io, pn_socket_t *dest);
PN_EXTERN ssize_t pn_read(pn_io_t *io, pn_socket_t socket, void *buf, size_t size);
PN_EXTERN ssize_t pn_write(pn_io_t *io, pn_socket_t socket, const void *buf, size_t size);
PN_EXTERN bool pn_wouldblock(pn_io_t *io);
PN_EXTERN pn_selector_t *pn_io_selector(pn_io_t *io);

#ifdef __cplusplus
}
#endif

#endif /* io.h */
