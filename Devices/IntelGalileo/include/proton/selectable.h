#ifndef PROTON_SELECTABLE_H
#define PROTON_SELECTABLE_H 1

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
#include <proton/object.h>
#include <proton/io.h>
#include <proton/type_compat.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * @file
 *
 * The selectable API provides an interface for integration with third
 * party event loops.
 *
 * @defgroup selectable Selectable
 * @{
 */

/**
 * An iterator for selectables.
 */
typedef pn_iterator_t pn_selectables_t;

/**
 * A selectable object provides an interface that can be used to
 * incorporate proton's I/O into third party event loops.
 *
 * Every selectable is associated with exactly one file descriptor.
 * Selectables may be interested in three kinds of events, read
 * events, write events, and timer events. A selectable will express
 * its interest in these events through the ::pn_selectable_capacity(),
 * ::pn_selectable_pending(), and ::pn_selectable_deadline() calls.
 *
 * When a read, write, or timer event occurs, the selectable must be
 * notified by calling ::pn_selectable_readable(),
 * ::pn_selectable_writable(), and ::pn_selectable_expired() as
 * appropriate.
 *
 * Once a selectable reaches a terminal state (see
 * ::pn_selectable_is_terminal()), it will never be interested in
 * events of any kind. When this occurs it should be removed from the
 * external event loop and discarded using ::pn_selectable_free().
 */
typedef struct pn_selectable_t pn_selectable_t;

/**
 * Construct a new selectables iterator.
 *
 * @return a pointer to a new selectables iterator
 */
PN_EXTERN pn_selectables_t *pn_selectables(void);

/**
 * Get the next selectable from an iterator.
 *
 * @param[in] selectables a selectable iterator
 * @return the next selectable from the iterator
 */
PN_EXTERN pn_selectable_t *pn_selectables_next(pn_selectables_t *selectables);

/**
 * Free a selectables iterator.
 *
 * @param[in] selectables a selectables iterator (or NULL)
 */
PN_EXTERN void pn_selectables_free(pn_selectables_t *selectables);

/**
 * Get the file descriptor associated with a selectable.
 *
 * @param[in] selectable a selectable object
 * @return the file descriptor associated with the selectable
 */
PN_EXTERN pn_socket_t pn_selectable_fd(pn_selectable_t *selectable);

/**
 * Get the capacity of a selectable.
 *
 * A selectable with a positive capacity is interested in being
 * notified of read events. A negative capacity indicates that the
 * selectable will never be interested in read events ever again.
 *
 * @param[in] selectable a selectable object
 * @return the selectables capacity
 */
PN_EXTERN ssize_t pn_selectable_capacity(pn_selectable_t *selectable);

/**
 * Get the number of bytes pending for a selectable.
 *
 * A selectable with pending bytes is interested in being notified of
 * write events. If this value is negative then the selectable will
 * never be interested in write events ever again.
 *
 * @param[in] selectable a selectable object
 * @return the number of bytes pending for the selectable
 */
PN_EXTERN ssize_t pn_selectable_pending(pn_selectable_t *selectable);

/**
 * Get the next deadline for a selectable.
 *
 * A selectable with a deadline is interested in being notified when
 * that deadline expires. Zero indicates there is currently no
 * deadline.
 *
 * @param[in] selectable a selectable object
 * @return the next deadline or zero
 */
PN_EXTERN pn_timestamp_t pn_selectable_deadline(pn_selectable_t *selectable);

/**
 * Notify a selectable that the file descriptor is readable.
 *
 * @param[in] selectable a selectable object
 */
PN_EXTERN void pn_selectable_readable(pn_selectable_t *selectable);

/**
 * Notify a selectable that the file descriptor is writable.
 *
 * @param[in] selectable a selectable object
 */
PN_EXTERN void pn_selectable_writable(pn_selectable_t *selectable);

/**
 * Notify a selectable that its deadline has expired.
 *
 * @param[in] selectable a selectable object
 */
PN_EXTERN void pn_selectable_expired(pn_selectable_t *selectable);

/**
 * Check if a selectable is registered.
 *
 * This flag is set via ::pn_selectable_set_registered() and can be
 * used for tracking whether a given selectable has been registerd
 * with an external event loop.
 *
 * @param[in] selectable
 * @return true if the selectable is registered
 */
PN_EXTERN bool pn_selectable_is_registered(pn_selectable_t *selectable);

/**
 * Set the registered flag for a selectable.
 *
 * See ::pn_selectable_is_registered() for details.
 *
 * @param[in] selectable a selectable object
 * @param[in] registered the registered flag
 */
PN_EXTERN void pn_selectable_set_registered(pn_selectable_t *selectable, bool registered);

/**
 * Check if a selectable is in the terminal state.
 *
 * A selectable that is in the terminal state will never be interested
 * in being notified of events of any kind ever again. Once a
 * selectable reaches this state it should be removed from any
 * external I/O loops and freed in order to reclaim any resources
 * associated with it.
 *
 * @param[in] selectable a selectable object
 * @return true if the selectable is in the terminal state, false otherwise
 */
PN_EXTERN bool pn_selectable_is_terminal(pn_selectable_t *selectable);

/**
 * Free a selectable object.
 *
 * @param[in] selectable a selectable object (or NULL)
 */
PN_EXTERN void pn_selectable_free(pn_selectable_t *selectable);

/**
 * @}
 */

#ifdef __cplusplus
}
#endif

#endif /* selectable.h */
