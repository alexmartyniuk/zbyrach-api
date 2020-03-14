import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from '../models/user';
import { Tag } from '../models/tag';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  constructor(private http: HttpClient) { }

  private baseUrl = 'https://localhost:5001/';

  public async Login(googleResponse: any): Promise<LoginResponse> {
    return this.http
        .post<LoginResponse>(this.baseUrl + 'account/login', googleResponse)
        .toPromise();
  }

  public async Logout(): Promise<any> {
      return this.http
        .post(this.baseUrl + 'account/logout', null)
        .toPromise();
  }

  public async GetRelatedTags(name: string): Promise<Tag[]> {
    const url = this.baseUrl + `tags/${name}/related`;
    return this.http
      .get<Tag[]>(url)
      .toPromise();
  }

  public async GetMyTags(): Promise<Tag[]> {
    const url = this.baseUrl +'tags/my';
    return this.http
      .get<Tag[]>(url)
      .toPromise();
  }

  public async SetMyTags(tags: string[]): Promise<any> {
    const url = this.baseUrl +'tags/my';
    return this.http.post(url, tags).toPromise();
  }
}

class LoginResponse {
  authToken: string;
  user: User;
}
