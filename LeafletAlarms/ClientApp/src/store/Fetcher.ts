import UserService from '../auth/UserService';

export  async function DoFetch(input: RequestInfo, init?: RequestInit): Promise<Response>
{
  if (UserService.isLoggedIn()) {
    const requestHeaders: HeadersInit = new Headers();
    requestHeaders.set('Authorization', 'Bearer ' + UserService.getToken());

    if (init == null) {
      init = {
        headers: requestHeaders
      };
    }
    else {
      if (init.headers == null) {
        init.headers = requestHeaders;
      }
      else {
        init.headers =
        {
          ...init.headers,
          Authorization: 'Bearer ' + UserService.getToken()
        }
      }
      init.headers
    }
  }

  var fetched = await fetch(input, init);

  return fetched;
}