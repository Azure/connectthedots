#ifndef PROTON_CID_H
#define PROTON_CID_H 1
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

typedef enum {
  CID_pn_object = 1,
  CID_pn_void,
  CID_pn_weakref,

  CID_pn_string,
  CID_pn_list,
  CID_pn_map,
  CID_pn_hash,

  CID_pn_collector,
  CID_pn_event,

  CID_pn_encoder,
  CID_pn_decoder,
  CID_pn_data,

  CID_pn_connection,
  CID_pn_session,
  CID_pn_link,
  CID_pn_delivery,
  CID_pn_transport,

  CID_pn_message,

  CID_pn_io,
  CID_pn_selector,
  CID_pn_selectable,

  CID_pn_url
} pn_cid_t;

#endif /* cid.h */
