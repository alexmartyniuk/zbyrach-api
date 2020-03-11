import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class TokenInterceptorService implements HttpInterceptor {

  constructor(private accountService: AccountService) { }
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.accountService.GetToken();
    let newHeaders = req.headers;
    if (token) {
      newHeaders = newHeaders.append('AuthToken', token);
    }

    const authRequest = req.clone({ headers: newHeaders });
    return next.handle(authRequest);
  }
}
