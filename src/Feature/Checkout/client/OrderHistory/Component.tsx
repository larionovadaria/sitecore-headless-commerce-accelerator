//    Copyright 2019 EPAM Systems, Inc.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

import * as React from 'react';

import { Text } from '@sitecore-jss/sitecore-jss-react';
import { OrderHistoryProps, OrderHistoryState } from './models';

import * as Jss from 'Foundation/ReactJss/client';

import './styles.scss';

import { OrderHistoryItem } from './components';

export class OrderHistoryControl extends Jss.SafePureComponent<OrderHistoryProps, OrderHistoryState> {
  constructor(props: OrderHistoryProps) {
    super(props);
  }

  public componentDidMount() {
    this.props.GetOrderHistory();
  }

  public safeRender() {
    const { orders, isLastPage, isLoading } = this.props;
    return (
      <div className="order-history">
        <div className="rainbow-title">
          <Text tag="h1" field={{ value: 'Order History' }} className="title" />
          <div className="rainbow-bar rainbow-bar-4" />
        </div>
        <div className="order-list">
          {orders &&
            orders.map((order, index) => (
              <div className="row order-list-item" key={index}>
                <OrderHistoryItem {...order} />
              </div>
            ))}
        </div>
        {isLoading && (
          <div className="order-history-loader">
            <div className="object object-one" />
            <div className="object object-two" />
            <div className="object object-three" />
          </div>
        )}
        {!isLastPage && !isLoading &&  (
          <div className="order-history-loadMore">
            <a className="btn btn-outline-green btn-block btn-load-more" href="#" onClick={(e) => this.loadMoreHandler(e)}>
              Load more
          </a>
          </div>
        )}
      </div>
    );
  }

  private loadMoreHandler(e: React.MouseEvent<HTMLAnchorElement>) {
    e.preventDefault();
    this.props.LoadMore();
  }
}
