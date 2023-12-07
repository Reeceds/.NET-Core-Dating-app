import { ResolveFn } from '@angular/router';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';
import { inject } from '@angular/core';

// Used to pass data on the root
export const memberDetailedResolver: ResolveFn<Member> = (route, state) => {
  const memberService = inject(MembersService); // 'Inject' is another method of dependancy injection instead of constructor

  return memberService.getMember(route.paramMap.get('username')!); // The MembersService and 'Member' data is activated by the root so that it can be passed to a component before the component is constructed
};
