import { Router } from '@angular/router';
import { PodcastModel } from 'app/models/podcasts.models';
import { Injectable } from '@angular/core';
import { Actions, Effect } from '@ngrx/effects';
import { Observable } from 'rxjs/Observable';
import { PodcastService } from 'app/services/podcast.service';
import * as podcasts from 'app/actions/podcast.actions';
import 'rxjs/add/operator/switchMap';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/of';
import 'rxjs/add/operator/do';

@Injectable()
export class PodcastsEffects {


    @Effect() get$ = this.actions$
        .ofType(podcasts.LOAD_PODCASTS)
        .switchMap(payload => this.podcastsService.get())
        .map(res => ({ type: podcasts.LOAD_PODCASTS_SUCCESS, payload: res }))
        .do((res) => this.router.navigate(['/podcasts', res.payload[0].slug]))
        .catch(() => Observable.of({ type: podcasts.LOAD_PODCASTS_FAIL }));

    @Effect() addPodcast$ = this.actions$
        .ofType(podcasts.ADD_PODCAST)
        .switchMap((action: podcasts.AddPodcastAction) => this.podcastsService.addPodcast(action.payload))
        .map(res => ({ type: podcasts.ADD_PODCAST_SUCCESS, payload: res }))
        .do((res) => this.router.navigate(['/podcasts', res.payload.slug]));
    @Effect() getPodcast$ = this.actions$
        .ofType(podcasts.GET_PODCAST)
        .switchMap((action: podcasts.GetPodcastAction) => this.podcastsService.getPodcast(action.payload))
        .map(res => ({ type: podcasts.GET_PODCAST_SUCCESS, payload: res }))
        .catch(() => Observable.of({ type: podcasts.GET_PODCAST_FAIL }));

    constructor(
        private actions$: Actions,
        private router: Router,
        private podcastsService: PodcastService
    ) { }
}
