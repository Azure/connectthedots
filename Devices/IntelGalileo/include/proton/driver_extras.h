#ifndef PROTON_DRIVER_H_EXTRAS
#define PROTON_DRIVER_H_EXTRAS 1

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

#ifdef __cplusplus
extern "C" {
#endif

#include <proton/import_export.h>
#include <proton/io.h>

/** @file
 * Additional API for the Driver Layer.
 *
 * These additional driver functions allow the application to supply
 * a separately created socket to the driver library.
 *
 */

/** Create a listener using the existing file descriptor.
 *
 * @param[in] driver driver that will 'own' this listener
 * @param[in] fd existing socket for listener to listen on
 * @param[in] context application-supplied, can be accessed via
 *                    pn_listener_context()
 * @return a new listener on the given host:port, NULL if error
 */
PN_EXTERN pn_listener_t *pn_listener_fd(pn_driver_t *driver, pn_socket_t fd, void *context);

PN_EXTERN pn_socket_t pn_listener_get_fd(pn_listener_t *listener);

/** Create a connector using the existing file descriptor.
 *
 * @param[in] driver driver that will 'own' this connector.
 * @param[in] fd existing socket to use for this connector.
 * @param[in] context application-supplied, can be accessed via
 *                    pn_connector_context()
 * @return a new connector to the given host:port, NULL if error.
 */
PN_EXTERN pn_connector_t *pn_connector_fd(pn_driver_t *driver, pn_socket_t fd, void *context);

PN_EXTERN pn_socket_t pn_connector_get_fd(pn_connector_t *connector);

#ifdef __cplusplus
}
#endif

#endif /* driver_extras.h */
