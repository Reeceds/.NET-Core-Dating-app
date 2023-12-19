import {
  ActivatedRouteSnapshot,
  DetachedRouteHandle,
  RouteReuseStrategy,
} from '@angular/router';

// RouteReuseStrategy can stop a component from being destroyed after use, can act as a sort of cache. Added as a provider in app.module.ts. Forces the member-messages component to display recipient messages from toaster notification click if the user is currently on a DIFFERENT recipients profile
export class CustomRouteReuseStrategy implements RouteReuseStrategy {
  shouldDetach(route: ActivatedRouteSnapshot): boolean {
    return false;
  }

  store(
    route: ActivatedRouteSnapshot,
    handle: DetachedRouteHandle | null
  ): void {}

  shouldAttach(route: ActivatedRouteSnapshot): boolean {
    return false;
  }

  retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle | null {
    return null;
  }

  shouldReuseRoute(
    future: ActivatedRouteSnapshot,
    curr: ActivatedRouteSnapshot
  ): boolean {
    return false;
  }
}
