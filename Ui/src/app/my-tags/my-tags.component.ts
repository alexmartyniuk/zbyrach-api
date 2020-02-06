import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-my-tags',
  templateUrl: './my-tags.component.html',
  styleUrls: ['./my-tags.component.css']
})
export class MyTagsComponent implements OnInit {

  constructor(private http: HttpClient) { }

  ngOnInit() {
  }

  currentTag: String = "";

  relatedTags: Array<String> = new Array<String>();

  tags: Array<String> = new Array<String>(); 

  async addTag(): Promise<any> {
    if (!this.currentTag) {
      return;
    }

    if (this.tags.includes(this.currentTag)) {
      return;
    }

    this.tags.push(this.currentTag);
    await this.getRelatedTags(this.currentTag);

    this.currentTag = "";
  }

  onRemove(event): void {
    console.log(event);
  }

  onClick(event): void {
    console.log(event);
  }

  private async getRelatedTags(tag: String): Promise<void> {   
    const url = `http://localhost:5000/tags/${tag}/related`;
    let relatedTags: Tag[] = await this.http.get<Tag[]>(url).toPromise();
    let tagNames = relatedTags.map(tag => tag.url);

    this.relatedTags.concat(tagNames);
  }

}

class Tag{
  name: string;
  url: string;
}
