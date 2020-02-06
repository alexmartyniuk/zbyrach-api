import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Tag } from '../models/tag';

@Component({
  selector: 'app-my-tags',
  templateUrl: './my-tags.component.html',
  styleUrls: ['./my-tags.component.css']
})
export class MyTagsComponent implements OnInit {

  constructor(private http: HttpClient) { }

  ngOnInit() {
  }

  currentTagName: string = "";

  relatedTags: Map<string, Tag> = new Map<string, Tag>();

  tags: Map<string, Tag> = new Map<string, Tag>();

  async addCurrentTag(): Promise<any> {
    if (!this.currentTagName) {
      return;
    }

    if (!this.tags.get(this.currentTagName)) {
      let newTag: Tag = {'name': this.currentTagName, 'url': null, 'parentTagName': null};
      this.tags.set(this.currentTagName, newTag);
      await this.getRelatedTags(newTag);
    }

    this.currentTagName = "";
  }

  async addTag(tag: Tag): Promise<any> {
    if (!this.tags.get(tag.name)) {
      this.tags.set(tag.name, tag);
      await this.getRelatedTags(tag);
    }

    this.currentTagName = "";
  }

  async addRelatedTag(tag: Tag): Promise<any> {
    if (!this.relatedTags.get(tag.name)) {
      this.relatedTags.set(tag.name, tag);
    }
  }

  onRemoveTag(tag: Tag): void {
    this.tags.delete(tag.name);
    this.relatedTags.delete(tag.name);
  }

  onClickTag(tag: Tag): void {
    //
  }

  onRemoveRelatedTag(tag: Tag): void {
    this.relatedTags.delete(tag.name);
  }

  onClickRelatedTag(tag: Tag): void {
    this.addTag(tag);
    this.onRemoveRelatedTag(tag);
  }

  private async getRelatedTags(tag: Tag): Promise<void> {
    const url = `http://localhost:5000/tags/${tag.name}/related`;
    let relatedTags: Tag[] = await this.http.get<Tag[]>(url).toPromise();

    for (let relatedTag of relatedTags) {
      relatedTag.parentTagName = tag.name;
      await this.addRelatedTag(relatedTag);
    }
  }

}
