import { Injectable } from '@angular/core';
import { Tag } from '../models/tag';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class TagService {

  constructor(private http: HttpClient) { }

  public async getRelatedTags(name: string): Promise<Tag[]> {
    const url = `http://localhost:5000/tags/${name}/related`;
    let relatedTags: Tag[] = await this.http.get<Tag[]>(url).toPromise();

    for (let relatedTag of relatedTags) {
      relatedTag.parentTagName = name;
    }

    return relatedTags;
  }

  public async getMyTags(): Promise<Tag[]> {
    const url = `http://localhost:5000/tags/my`;
    return this.http.get<Tag[]>(url).toPromise();
  }

  public async setMyTags(tags: string[]): Promise<void> {
    const url = `http://localhost:5000/tags/my`;
    await this.http.post(url, tags).toPromise();
  }
}
