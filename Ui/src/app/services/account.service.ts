import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GoogleLoginProvider, AuthService } from 'angularx-social-login';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http: HttpClient, private authService: AuthService) { }

  private baseUrl = 'https://localhost:5001/';

  public async Login(): Promise<User> {
    try {
      const googleResponse = await this.authService.signIn(GoogleLoginProvider.PROVIDER_ID);
      const response = await this.http
        .post<LoginResponse>(this.baseUrl + 'account/login', googleResponse).toPromise();

      this.SetToken(response.authToken);
      this.SetUser(response.user);
      return Promise.resolve(response.user);
    } catch (e) {
      return Promise.reject(e);
    }
  }

  public async Logout(): Promise<any> {
    try {
      await this.authService.signOut();
      await this.http.post(this.baseUrl + 'account/logout', null).toPromise();

      this.RemoveToken();
      this.RemoveUser();
      return Promise.resolve();
    } catch (e) {
      return Promise.reject(e);
    }
  }

  public GetToken(): string {
    return localStorage.getItem('token');
  }

  private SetToken(token: string) {
    localStorage.setItem('token', token);
  }

  private RemoveToken() {
    localStorage.removeItem('token');
  }

  public GetUser(): User {
    return JSON.parse(localStorage.getItem('user'));
  }

  private SetUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
  }

  private RemoveUser() {
    localStorage.removeItem('user');
  }
}

class User {
  id: number;
  name: string;
  email: string;
  pictureUrl: string;
}

class LoginResponse {
  authToken: string;
  user: User;
}
