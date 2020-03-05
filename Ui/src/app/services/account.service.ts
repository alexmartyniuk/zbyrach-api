import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http: HttpClient) { }

  private baseUrlLogin = 'https://localhost:5001/account/login';

  public Login(loginData) {
    return this.http.post<any>(this.baseUrlLogin, loginData);
  }

}
