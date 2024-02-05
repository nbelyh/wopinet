import { Component } from '@angular/core';

interface IWopiFile {
  fileId: string;
  fileName: string;
  fileExtension: string;
  viewUrl: string;
  editUrl: string;
  token: string;
  tokenTtl: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {

  API = 'https://live-dingo-upright.ngrok-free.app';

  onSubmit (evt: any) {
    var input = document.getElementById('file') as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      var fd = new FormData();
      fd.append('file', input.files[0]);
      fetch(`${this.API}/upload`, {
        method: 'POST',
        body: fd
      }).then(res => {
        if (res.ok) {
          this.fetchFiles();
        }
      });
    }
  }

  onOpenFrame (file: IWopiFile, propName: 'viewUrl' | 'editUrl') {
    var frameholder = document.getElementById("frameholder") as HTMLDivElement;
    frameholder.innerHTML = "";

    var office_frame = document.createElement("iframe") as HTMLIFrameElement;
    office_frame.name = "office_frame";
    office_frame.id = "office_frame";
    office_frame.style.width = "100%";
    office_frame.style.height = "100%";

    var office_form = document.createElement("form") as HTMLFormElement;
    office_form.action = file[propName];
    office_form.method = "post";
    office_form.id = "office_form";
    office_form.name = "office_form";
    office_form.target = "office_frame";

    var access_token = document.createElement("input") as HTMLInputElement;
    access_token.name = "access_token";
    access_token.value = file.token;
    access_token.type = "hidden";
    office_form.appendChild(access_token);

    var access_token_ttl = document.createElement("input") as HTMLInputElement;
    access_token_ttl.name = "access_token_ttl";
    access_token_ttl.value = file.tokenTtl;
    access_token_ttl.type = "hidden";
    office_form.appendChild(access_token_ttl);

    frameholder.appendChild(office_form);
    frameholder.appendChild(office_frame);
    office_form.submit();
  }

  onView (file: IWopiFile) {
    this.onOpenFrame(file, 'viewUrl')
  }

  onEdit (file: IWopiFile) {
    this.onOpenFrame(file, 'editUrl')
  }

  onDelete (file: IWopiFile) {
    fetch(`${this.API}/delete/${file.fileId}`, {
      method: 'POST'
    }).then(res => {
      if (res.ok) {
        this.fetchFiles();
      }
    });
  }

  fetchFiles () {
    fetch(`${this.API}/api/Files`)
    .then(res => res.json())
    .then(data => this.files = data);
  }

  ngOnInit () {
    this.fetchFiles();
    window.addEventListener('message', (e) => {
      this.events.push(JSON.stringify(e.data));
    }, false);

  }

  clearEvents() {
    this.events = [];
  }

  events: any[] = [];
  files: IWopiFile[] = []
}
